using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Client.Core.Features.Core.SchoolClasses;
using SchoolJournal.Client.Core.Features.Core.Subject;
using SchoolJournal.Client.Core.Features.Core.Teachers;
using SchoolJournal.Contracts.DTOs.Core.SchoolClasses;
using SchoolJournal.Contracts.DTOs.Core.Subjects;
using SchoolJournal.Contracts.DTOs.Core.Teachers;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;
using SchoolJournal.Client.Core.Common.Auth;

namespace SchoolJournal.Client.Core.Features.Operations.Quizzes;

public enum AiQuizType
{
    MultipleChoice,
    TrueFalse,
    OddOneOut,
    GuessByDescription,
    Proofreader,
    AssociativeBush,
    Crossword,
    Fillword,
    FillInTheBlank,
    Matching
}

public sealed partial class AiQuizGeneratorViewModel(
    IAiGenerationApi aiGenerationApi,
    IQuizzesApi quizzesApi,
    ISubjectApi subjectApi,
    ISchoolClassApi classApi,
    ITeacherApi teacherApi,
    ITokenStorageService tokenStorageService) : ObservableObject
{
    private const string ProblemTitleKey = "title";

    [ObservableProperty] public partial bool IsLoading { get; set; }
    [ObservableProperty] public partial string? ErrorMessage { get; set; }
    [ObservableProperty] public partial string? SuccessMessage { get; set; }

    [ObservableProperty] public partial string? SelectedFileName { get; set; }

    // Фабрика для безпечного створення нових потоків (кросплатформно: WPF та Blazor)
    public Func<Stream>? FileStreamFactory { get; set; }

    [ObservableProperty] public partial int TotalQuestions { get; set; } = 10;
    [ObservableProperty] public partial int PointsPerQuestion { get; set; } = 1;
    [ObservableProperty] public partial int? StartPage { get; set; }
    [ObservableProperty] public partial int? EndPage { get; set; }

    [ObservableProperty] public partial int MultiAnswerCount { get; set; } = 3;
    [ObservableProperty] public partial int JudgmentCount { get; set; } = 3;

    [ObservableProperty] public partial GeneratedQuizResponse? GeneratedQuiz { get; set; }

    [ObservableProperty] public partial string GeneratedQuizTitle { get; set; } = string.Empty;

    [ObservableProperty] public partial ObservableCollection<SubjectResponse> Subjects { get; set; } = [];
    [ObservableProperty] public partial ObservableCollection<SchoolClassItemResponse> Classes { get; set; } = [];

    [ObservableProperty] public partial Guid? SelectedSubjectId { get; set; }
    [ObservableProperty] public partial Guid? SelectedClassId { get; set; }

    public IReadOnlyList<AiQuizType> QuizTypes { get; } = Enum.GetValues<AiQuizType>();
    [ObservableProperty] public partial AiQuizType SelectedQuizType { get; set; } = AiQuizType.MultipleChoice;

    public bool IsMultipleChoiceSelected => SelectedQuizType == AiQuizType.MultipleChoice;

    public async Task InitializeAsync()
    {
        await LoadInitialDataCommand.ExecuteAsync(null).ConfigureAwait(true);
    }

    partial void OnSelectedQuizTypeChanged(AiQuizType value)
    {
        OnPropertyChanged(nameof(IsMultipleChoiceSelected));
    }

    [RelayCommand]
    private async Task<Guid?> ResolveTeacherIdAsync(CancellationToken ct)
    {
        var (accessToken, _) = await tokenStorageService.GetTokensAsync(ct).ConfigureAwait(true);
        Guid userId = ExtractUserIdFromJwt(accessToken);

        if (userId == Guid.Empty)
        {
            ErrorMessage = "Помилка безпеки: неможливо витягнути UserId з токена.";
            return null;
        }

        var teacherResponse = await teacherApi.GetTeacherByUserIdAsync(userId, ct).ConfigureAwait(true);

        if (!teacherResponse.IsSuccessStatusCode || teacherResponse.Content is null)
        {
            ErrorMessage = $"Профіль вчителя не знайдено для цього акаунта (UserId: {userId}). Зверніться до адміністратора.";
            return null;
        }

        return teacherResponse.Content.TeacherId;
    }

    [RelayCommand]
    private async Task LoadInitialDataAsync(CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            Guid? teacherId = await ResolveTeacherIdAsync(ct).ConfigureAwait(true);
            if (teacherId is null) return;

            var classesTask = classApi.GetClassesByTeacherAsync(teacherId.Value, ct);
            var subjectsTask = subjectApi.GetSubjectsByTeacherAsync(teacherId.Value, ct);

            await Task.WhenAll(classesTask, subjectsTask).ConfigureAwait(true);

            var classesResult = await classesTask.ConfigureAwait(true);
            var subjectsResult = await subjectsTask.ConfigureAwait(true);

            Classes.Clear();
            foreach (var item in classesResult.Content ?? []) Classes.Add(item);

            Subjects.Clear();
            foreach (var item in subjectsResult.Content ?? []) Subjects.Add(item);

            if (Classes.Count == 0 || Subjects.Count == 0)
            {
                ErrorMessage = "Дані отримано, але списки порожні. Перевірте, чи є у цього вчителя призначені класи/предмети, та чи збігається TeacherId з базою.";
            }
        }
        catch (ApiException ex)
        {
            ErrorMessage = $"Помилка API при завантаженні довідників: {ex.StatusCode}";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Помилка мережі при завантаженні довідників: {ex.Message}";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Системна помилка при завантаженні: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSelectedClassIdChanged(Guid? value)
    {
        // Оскільки ми тепер вантажимо тільки предмети вчителя на етапі ініціалізації сторінки,
        // нам більше не потрібно робити додатковий запит до БД при зміні класу.
        SelectedSubjectId = null;
    }

    [RelayCommand]
    private async Task GenerateQuizAsync(AiQuizType quizType, CancellationToken ct)
    {
        if (FileStreamFactory is null || string.IsNullOrWhiteSpace(SelectedFileName))
        {
            ErrorMessage = "Будь ласка, завантажте валідний PDF файл.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;
        GeneratedQuiz = null;

        try
        {
            // Fail Fast: Створюємо новий потік безпосередньо перед запитом. 
            // Якщо HttpClient його закриє, при Retry просто створиться новий потік!
            using var fileStream = FileStreamFactory.Invoke();
            var streamPart = new StreamPart(fileStream, SelectedFileName, "application/pdf");
            
            IApiResponse<GeneratedQuizResponse> response = quizType switch
            {
                AiQuizType.MultipleChoice => await aiGenerationApi.GenerateMultipleChoiceFromPdfAsync(streamPart, TotalQuestions, MultiAnswerCount, JudgmentCount, PointsPerQuestion, StartPage, EndPage, ct).ConfigureAwait(true),
                AiQuizType.TrueFalse => await aiGenerationApi.GenerateTrueFalseFromPdfAsync(streamPart, TotalQuestions, PointsPerQuestion, StartPage, EndPage, ct).ConfigureAwait(true),
                AiQuizType.OddOneOut => await aiGenerationApi.GenerateOddOneOutFromPdfAsync(streamPart, TotalQuestions, PointsPerQuestion, StartPage, EndPage, ct).ConfigureAwait(true),
                AiQuizType.GuessByDescription => await aiGenerationApi.GenerateGuessByDescriptionFromPdfAsync(streamPart, TotalQuestions, PointsPerQuestion, StartPage, EndPage, ct).ConfigureAwait(true),
                AiQuizType.Proofreader => await aiGenerationApi.GenerateProofreaderFromPdfAsync(streamPart, TotalQuestions, PointsPerQuestion, StartPage, EndPage, ct).ConfigureAwait(true),
                AiQuizType.AssociativeBush => await aiGenerationApi.GenerateAssociativeBushFromPdfAsync(streamPart, TotalQuestions, PointsPerQuestion, StartPage, EndPage, ct).ConfigureAwait(true),
                AiQuizType.Crossword => await aiGenerationApi.GenerateCrosswordFromPdfAsync(streamPart, TotalQuestions, PointsPerQuestion, StartPage, EndPage, ct).ConfigureAwait(true),
                AiQuizType.Fillword => await aiGenerationApi.GenerateFillwordFromPdfAsync(streamPart, TotalQuestions, PointsPerQuestion, StartPage, EndPage, ct).ConfigureAwait(true),
                AiQuizType.FillInTheBlank => await aiGenerationApi.GenerateFillInTheBlankFromPdfAsync(streamPart, TotalQuestions, PointsPerQuestion, StartPage, EndPage, ct).ConfigureAwait(true),
                AiQuizType.Matching => await aiGenerationApi.GenerateMatchingFromPdfAsync(streamPart, TotalQuestions, PointsPerQuestion, StartPage, EndPage, ct).ConfigureAwait(true),
                _ => throw new ArgumentOutOfRangeException(nameof(quizType), "Невідомий тип тесту")
            };

            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                GeneratedQuiz = response.Content;
                GeneratedQuizTitle = GeneratedQuiz.Title ?? "Згенероване завдання";
                SuccessMessage = "Тест успішно згенеровано! Перевірте його та натисніть 'Зберегти'.";
            }
            else
            {
                ErrorMessage = await GetApiErrorMessageAsync(response.Error as ApiException).ConfigureAwait(true);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Системна помилка під час генерації: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveGeneratedQuizAsync(CancellationToken ct)
    {
        if (GeneratedQuiz is null)
        {
            ErrorMessage = "Немає згенерованого тесту для збереження.";
            return;
        }

        var (accessToken, _) = await tokenStorageService.GetTokensAsync(ct).ConfigureAwait(true);
        Guid userId = ExtractUserIdFromJwt(accessToken);

        if (userId == Guid.Empty)
        {
            ErrorMessage = "Критична помилка безпеки: сесія недійсна або відсутній ідентифікатор користувача.";
            return;
        }

        // Отримуємо TeacherId ще раз перед збереженням
        var teacherResponse = await teacherApi.GetTeacherByUserIdAsync(userId, ct).ConfigureAwait(true);
        if (!teacherResponse.IsSuccessStatusCode || teacherResponse.Content is null)
        {
            ErrorMessage = "Помилка: ваш профіль вчителя не знайдено.";
            return;
        }

        Guid realTeacherId = teacherResponse.Content.TeacherId;

        if (!SelectedSubjectId.HasValue || !SelectedClassId.HasValue)
        {
            ErrorMessage = "Оберіть предмет та клас перед збереженням.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            var questionsRequest = GeneratedQuiz.Questions.Select(q => new SaveGeneratedQuizQuestionRequest(
                            q.OrderIndex,
                            q.QuestionText,
                            q.QuestionType,
                            q.ContentJson,
                            q.Points
                        )).ToList();

            var request = new SaveGeneratedQuizRequest(
                                        realTeacherId,
                                        SelectedSubjectId.Value,
                                        SelectedClassId.Value,
                                        string.IsNullOrWhiteSpace(GeneratedQuizTitle) ? "Згенероване завдання" : GeneratedQuizTitle,
                                        questionsRequest
                                    );

            var response = await quizzesApi.SaveGeneratedQuizAsync(request, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                SuccessMessage = "✅ Тест успішно збережено в базу та призначено класу!";
                GeneratedQuiz = null; 
            }
            else
            {
                ErrorMessage = await GetApiErrorMessageAsync(response.Error as ApiException).ConfigureAwait(true);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка збереження: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static async Task<string> GetApiErrorMessageAsync(ApiException? error)
    {
        if (error is null)
        {
            return "Невідома помилка сервера.";
        }

        var problem = await error.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true);
        return problem?[ProblemTitleKey]?.ToString() ?? "Помилка обробки запиту.";
    }

    private static Guid ExtractUserIdFromJwt(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return Guid.Empty;
        var parts = token.Split('.');
        if (parts.Length < 2) return Guid.Empty;

        try
        {
            var payload = parts[1]
                .PadRight(parts[1].Length + (4 - parts[1].Length % 4) % 4, '=')
                .Replace('-', '+')
                .Replace('_', '/');

            var jsonBytes = Convert.FromBase64String(payload);
            using var doc = System.Text.Json.JsonDocument.Parse(jsonBytes);
            var root = doc.RootElement;

            if (root.TryGetProperty("nameid", out var idProp) && Guid.TryParse(idProp.GetString(), out var id1)) return id1;
            if (root.TryGetProperty("sub", out var subProp) && Guid.TryParse(subProp.GetString(), out var id2)) return id2;
            if (root.TryGetProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", out var longProp) && Guid.TryParse(longProp.GetString(), out var id3)) return id3;
        }
        catch (FormatException)
        {
            // Токен має невалідний Base64 формат (пошкоджено)
        }
        catch (System.Text.Json.JsonException)
        {
            // Внутрішня частина токена не є валідним JSON
        }

        return Guid.Empty;
    }
}