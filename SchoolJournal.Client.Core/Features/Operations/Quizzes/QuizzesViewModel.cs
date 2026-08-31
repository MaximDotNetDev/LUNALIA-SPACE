using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Refit;
using SchoolJournal.Client.Core.Common.Auth;
using SchoolJournal.Client.Core.Features.Core.SchoolClasses;
using SchoolJournal.Client.Core.Features.Core.Subject;
using SchoolJournal.Client.Core.Features.Core.Teachers;
using SchoolJournal.Client.Core.Features.Operations.QuizAssignments;
using SchoolJournal.Contracts.DTOs.AiGeneration;
using SchoolJournal.Contracts.DTOs.Core.SchoolClasses;
using SchoolJournal.Contracts.DTOs.Core.Subjects;
using SchoolJournal.Contracts.DTOs.Core.Teachers;
using SchoolJournal.Contracts.DTOs.Operations.QuizAssignments;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;
using System.Collections.ObjectModel;
using System.Text.Json.Nodes;
using QuizQuestionResponse = SchoolJournal.Contracts.DTOs.Operations.Quizzes.QuizQuestionResponse;
using UpdateQuizQuestionRequest = SchoolJournal.Contracts.DTOs.Operations.Quizzes.UpdateQuizQuestionRequest;

namespace SchoolJournal.Client.Core.Features.Operations.Quizzes;

public sealed partial class QuizzesViewModel : ObservableObject
{
    private const string ProblemTitleKey = "title";

    private readonly IQuizzesApi _quizzesApi;
    private readonly IQuizAssignmentsApi _quizAssignmentsApi;
    private readonly ISubjectApi _subjectApi;
    private readonly ISchoolClassApi _schoolClassApi;
    private readonly ITokenStorageService _tokenStorageService;

    public QuizzesViewModel(
        IQuizzesApi quizzesApi,
        IQuizAssignmentsApi quizAssignmentsApi,
        ISubjectApi subjectApi,
        ISchoolClassApi schoolClassApi,
        ITokenStorageService tokenStorageService)
    {
        _quizzesApi = quizzesApi;
        _quizAssignmentsApi = quizAssignmentsApi;
        _subjectApi = subjectApi;
        _schoolClassApi = schoolClassApi;
        _tokenStorageService = tokenStorageService;

        _ = LoadDictionariesAsync(CancellationToken.None);
        _ = LoadQuizzesAsync(CancellationToken.None);
    }

    [ObservableProperty]
    public partial ObservableCollection<QuizResponse> Quizzes { get; set; } = [];

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsFormOpen { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotEditing))]
    public partial bool IsEditing { get; set; }

    public bool IsNotEditing => !IsEditing;

    [ObservableProperty]
    public partial string FormTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<SubjectResponse> AvailableSubjects { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<SchoolClassItemResponse> AvailableClasses { get; set; } = [];

    [ObservableProperty]
    public partial Guid? FormQuizId { get; set; }

    [ObservableProperty]
    public partial string FormQuizTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial Guid? FormSubjectId { get; set; }

    private string? _formRowVersion;
    private IReadOnlyCollection<QuizQuestionResponse> _formQuestions = [];

    [ObservableProperty]
    public partial bool IsAssignFormOpen { get; set; }

    [ObservableProperty]
    public partial Guid AssignQuizId { get; set; }

    [ObservableProperty]
    public partial Guid? AssignClassId { get; set; }

    [ObservableProperty]
    public partial DateTime? AssignDueDate { get; set; }

    private async Task LoadDictionariesAsync(CancellationToken ct)
    {
        try
        {
            var subjectsTask = _subjectApi.GetActiveSubjectsAsync(1, 1000, null, ct);
            var classesTask = _schoolClassApi.GetActiveClassesAsync(1, 1000, null, ct);

            await Task.WhenAll(subjectsTask, classesTask).ConfigureAwait(true);

            var subjectsResponse = await subjectsTask.ConfigureAwait(true);
            var classesResponse = await classesTask.ConfigureAwait(true);

            if (subjectsResponse.IsSuccessStatusCode && subjectsResponse.Content is not null)
                AvailableSubjects = [.. subjectsResponse.Content.Items];

            if (classesResponse.IsSuccessStatusCode && classesResponse.Content is not null)
                AvailableClasses = [.. classesResponse.Content.Items];
        }
        catch (ApiException ex)
        {
            ErrorMessage = $"Помилка API при завантаженні довідників: {ex.StatusCode}";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Помилка мережі при завантаженні довідників: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task LoadQuizzesAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _quizzesApi.GetQuizzesPagedAsync(null, 1, 100, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                Quizzes = [.. response.Content.Items];
            }
        }
        catch (ApiException ex)
        {
            ErrorMessage = $"Помилка API при завантаженні тестів: {ex.StatusCode}";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Помилка мережі при завантаженні тестів: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    



    [RelayCommand]
    private async Task OpenEditFormAsync(QuizResponse quiz, CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _quizzesApi.GetQuizByIdAsync(quiz.QuizId, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                var freshQuiz = response.Content;
                IsEditing = true;
                FormTitle = "Редагування тесту";
                FormQuizId = freshQuiz.QuizId;
                FormQuizTitle = freshQuiz.Title;
                FormSubjectId = freshQuiz.SubjectId;
                _formRowVersion = freshQuiz.RowVersionBase64;
                _formQuestions = freshQuiz.Questions;
                IsFormOpen = true;
            }
            else
            {
                ErrorMessage = "Не вдалося завантажити актуальні дані тесту для редагування.";
            }
        }
        catch (ApiException ex)
        {
            ErrorMessage = $"Помилка API при отриманні тесту: {ex.StatusCode}";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Помилка мережі при отриманні тесту: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void CloseForm()
    {
        IsFormOpen = false;
        ErrorMessage = null;
    }

    [RelayCommand]
    private static void OpenQuestions(QuizResponse quiz)
    {
        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new OpenQuizQuestionsMessage(quiz.QuizId));
    }

    [RelayCommand]
    private async Task SaveFormAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(FormQuizTitle) || !FormSubjectId.HasValue)
        {
            ErrorMessage = "Будь ласка, заповніть всі обов'язкові поля (Назва, Предмет).";
            return;
        }

        var (accessToken, _) = await _tokenStorageService.GetTokensAsync(ct).ConfigureAwait(true);
        Guid teacherId = ExtractUserIdFromJwt(accessToken);

        if (teacherId == Guid.Empty)
        {
            ErrorMessage = "Критична помилка безпеки: сесія недійсна.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            IApiResponse response;

            if (IsEditing && FormQuizId.HasValue && _formRowVersion is not null)
            {
                var questionsToUpdate = _formQuestions.Select(q => new UpdateQuizQuestionRequest(
                    q.QuestionId,
                    q.OrderIndex,
                    q.QuestionText,
                    q.QuestionType,
                    q.ContentJson,
                    q.Points)).ToList();

                var request = new UpdateQuizRequest(FormSubjectId.Value, FormQuizTitle, _formRowVersion, questionsToUpdate);
                response = await _quizzesApi.UpdateQuizAsync(FormQuizId.Value, request, ct).ConfigureAwait(true);
            }
            else
            {
                if (_formQuestions.Count > 0)
                {
                    var request = new SaveGeneratedQuizRequest(
                        teacherId,
                        FormSubjectId.Value,
                        Guid.Empty,
                        FormQuizTitle,
                        [.. _formQuestions.Select(q => new SaveGeneratedQuizQuestionRequest(
                            q.OrderIndex,
                            q.QuestionText,
                            q.QuestionType,
                            q.ContentJson,
                            q.Points))]
                    );
                    response = await _quizzesApi.SaveGeneratedQuizAsync(request, ct).ConfigureAwait(true);
                }
                else
                {
                    var request = new CreateQuizRequest(teacherId, FormSubjectId.Value, FormQuizTitle);
                    response = await _quizzesApi.CreateQuizAsync(request, ct).ConfigureAwait(true);
                }
            }
            if (response.IsSuccessStatusCode)
            {
                IsFormOpen = false;
                await LoadQuizzesAsync(ct).ConfigureAwait(true);
            }
            else if (response.Error is not null)
            {
var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                ErrorMessage = problem?[ProblemTitleKey]?.ToString() ?? "Помилка збереження даних тесту.";
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not StackOverflowException)
        {
            ErrorMessage = $"Системна помилка: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteQuizAsync(QuizResponse quiz, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var request = new DeleteQuizRequest(quiz.RowVersionBase64);
            var response = await _quizzesApi.DeleteQuizAsync(quiz.QuizId, request, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                Quizzes.Remove(quiz);
            }
            else if (response.Error is not null)
            {
var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                ErrorMessage = problem?[ProblemTitleKey]?.ToString() ?? "Помилка видалення.";
            }
        }
        catch (ApiException ex)
        {
            ErrorMessage = $"Помилка API при видаленні: {ex.StatusCode}";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Помилка мережі при видаленні: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void OpenAssignForm(QuizResponse quiz)
    {
        AssignQuizId = quiz.QuizId;
        AssignClassId = null;
        AssignDueDate = null;
        ErrorMessage = null;
        IsAssignFormOpen = true;
    }

    [RelayCommand]
    private void CloseAssignForm()
    {
        IsAssignFormOpen = false;
        ErrorMessage = null;
    }

    [RelayCommand]
    private static void OpenAiGenerator()
    {
        WeakReferenceMessenger.Default.Send(new OpenAiGeneratorMessage());
    }

    [RelayCommand]
    private async Task SaveAssignFormAsync(CancellationToken ct)
    {
        if (!AssignClassId.HasValue)
        {
            ErrorMessage = "Будь ласка, оберіть клас зі списку.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            DateTimeOffset? dueDateUtc = AssignDueDate.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(AssignDueDate.Value, DateTimeKind.Utc))
                : null;

            var request = new CreateQuizAssignmentRequest(AssignQuizId, AssignClassId.Value, dueDateUtc);
            var response = await _quizAssignmentsApi.CreateQuizAssignmentAsync(request, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                IsAssignFormOpen = false;
            }
            else if (response.Error is not null)
            {
                var problem = response.Error is ApiException apiEx
                    ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true)
                    : null;
                ErrorMessage = problem?[ProblemTitleKey]?.ToString() ?? "Помилка призначення тесту.";
            }
        }
        catch (ApiException ex)
        {
            ErrorMessage = $"Помилка API при призначенні: {ex.StatusCode}";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Помилка мережі при призначенні: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
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
            // Токен має пошкоджений Base64 формат
            return Guid.Empty;
        }
        catch (System.Text.Json.JsonException)
        {
            // Payload токена не є валідним JSON
            return Guid.Empty;
        }

        return Guid.Empty;
    }
}