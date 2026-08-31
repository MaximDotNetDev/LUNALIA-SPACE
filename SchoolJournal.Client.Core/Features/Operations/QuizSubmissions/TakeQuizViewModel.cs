using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Client.Core.Features.Operations.QuizQuestions;
using SchoolJournal.Client.Core.Features.Operations.QuizSubmissions;
using SchoolJournal.Contracts.DTOs.Operations.QuizQuestions;
using SchoolJournal.Contracts.DTOs.Operations.QuizSubmissions;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using SchoolJournal.Client.Core.Common.ViewModels;

namespace SchoolJournal.Client.Core.Features.Operations.QuizExecution;

/// <summary>
/// Модель для відображення загадки кросворду на екрані учня.
/// </summary>
public sealed partial class CrosswordWordItem : ObservableObject
{
    public int Number { get; init; }
    public string Clue { get; init; } = string.Empty;
    public string Direction { get; init; } = string.Empty;
}

/// <summary>
/// Модель для однієї клітинки сітки кросворду.
/// </summary>
public sealed partial class CrosswordCellItem : ObservableObject
{
    public int Row { get; init; }
    public int Column { get; init; }
    public bool IsEmpty { get; init; }
    public string Number { get; init; } = string.Empty;

    [ObservableProperty]
    public partial string UserInput { get; set; } = string.Empty;
}

/// <summary>
/// Модель для клітинки філворду.
/// </summary>
public sealed partial class FillwordCellItem : ObservableObject
{
    public int Row { get; init; }
    public int Column { get; init; }
    public char Letter { get; init; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    [ObservableProperty]
    public partial string SelectionColor { get; set; } = "Transparent";
}

/// <summary>
/// Модель для слова філворду, яке потрібно знайти.
/// </summary>
public sealed partial class FillwordWordItem : ObservableObject
{
    public int Number { get; init; }
    public string Clue { get; init; } = string.Empty;
}

/// <summary>
/// Модель для окремого варіанту відповіді (щоб прив'язати CheckBox).
/// </summary>
public sealed partial class QuizOptionItem(string text) : ObservableObject
{
    public string Text { get; } = text;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}

/// <summary>
/// Обгортка для питання, яка зберігає введену/обрану учнем відповідь на клієнті.
/// </summary>
public sealed partial class QuestionExecutionItem : ObservableObject
{
    public QuizQuestionResponse Question { get; }
    public ObservableCollection<QuizOptionItem> Options { get; } = [];
    public bool HasOptions => Options.Count > 0;

    [ObservableProperty]
    public partial bool IsMultipleChoice { get; set; }

    [ObservableProperty]
    public partial bool IsCrossword { get; set; }

    [ObservableProperty]
    public partial int CrosswordWidth { get; set; }

    public ObservableCollection<CrosswordCellItem> CrosswordCells { get; } = [];
    public ObservableCollection<CrosswordWordItem> CrosswordWords { get; } = [];

    [ObservableProperty]
    public partial bool IsFillword { get; set; }

    [ObservableProperty]
    public partial int FillwordWidth { get; set; }

    public ObservableCollection<FillwordCellItem> FillwordCells { get; } = [];
    public ObservableCollection<FillwordWordItem> FillwordWords { get; } = [];

    [ObservableProperty]
    public partial string UserAnswer { get; set; } = string.Empty;

    public QuestionExecutionItem(QuizQuestionResponse question)
    {
        ArgumentNullException.ThrowIfNull(question);
        Question = question;

        if (string.IsNullOrWhiteSpace(question.ContentJson)) return;

        if (question.QuestionType == 9)
        {
            IsCrossword = true;
            ParseCrosswordFromJson(question.ContentJson);
        }
        else if (question.QuestionType == 10)
        {
            IsFillword = true;
            ParseFillwordFromJson(question.ContentJson);
        }
        else if (question.QuestionType == 4)
        {
            ParseTrueFalseFromJson();
            EnforceSingleSelectionRule();
        }
        else
        {
            ParseOptionsFromJson(question.ContentJson);

            if (!IsMultipleChoice && HasOptions)
            {
                EnforceSingleSelectionRule();
            }
        }
    }

    private void ParseTrueFalseFromJson()
    {
        IsMultipleChoice = false;
        Options.Add(new QuizOptionItem("Так"));
        Options.Add(new QuizOptionItem("Ні"));
    }

    private void ParseFillwordFromJson(string json)
    {
        try
        {
            var node = System.Text.Json.Nodes.JsonNode.Parse(json);
            if (node is null) return;

            FillwordWidth = node["width"]?.GetValue<int>() ?? 0;

            if (node["words"] is System.Text.Json.Nodes.JsonArray wordsArray)
            {
                foreach (var w in wordsArray)
                {
                    FillwordWords.Add(new FillwordWordItem
                    {
                        Number = w?["number"]?.GetValue<int>() ?? 0,
                        Clue = w?["clue"]?.ToString() ?? string.Empty
                    });
                }
            }

            if (node["cells"] is System.Text.Json.Nodes.JsonArray cellsArray)
            {
                foreach (var c in cellsArray)
                {
                    var cell = new FillwordCellItem
                    {
                        Row = c?["row"]?.GetValue<int>() ?? 0,
                        Column = c?["column"]?.GetValue<int>() ?? 0,
                        Letter = c?["letter"]?.ToString().FirstOrDefault() ?? ' '
                    };

                    // Підписуємося на виділення для синхронізації відповіді
                    cell.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(FillwordCellItem.IsSelected))
                        {
                            SyncFillwordAnswer();
                        }
                    };

                    FillwordCells.Add(cell);
                }
            }
        }
        catch (System.Text.Json.JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Помилка парсингу філворду: {ex.Message}");
        }
    }

    private void SyncFillwordAnswer()
    {
        // Зберігаємо координати виділених клітинок у форматі JSON-масиву ["0_0", "0_1", ...]
        var selectedKeys = FillwordCells
            .Where(c => c.IsSelected)
            .Select(c => $"{c.Row}_{c.Column}")
            .ToArray();

        var arrayNode = new System.Text.Json.Nodes.JsonArray();
        foreach (var key in selectedKeys)
        {
            arrayNode.Add(System.Text.Json.Nodes.JsonValue.Create(key));
        }

        UserAnswer = arrayNode.ToJsonString();
    }

    private void ParseCrosswordFromJson(string json)
    {
        try
        {
            var node = System.Text.Json.Nodes.JsonNode.Parse(json);
            if (node is null) return;

            CrosswordWidth = node["width"]?.GetValue<int>() ?? 0;

            if (node["words"] is System.Text.Json.Nodes.JsonArray wordsArray)
            {
                foreach (var w in wordsArray)
                {
                    bool isHoriz = w?["isHorizontal"]?.GetValue<bool>() ?? true;
                    CrosswordWords.Add(new CrosswordWordItem
                    {
                        Number = w?["number"]?.GetValue<int>() ?? 0,
                        Clue = w?["clue"]?.ToString() ?? string.Empty,
                        Direction = isHoriz ? "По горизонталі" : "По вертикалі"
                    });
                }
            }

            if (node["cells"] is System.Text.Json.Nodes.JsonArray cellsArray)
            {
                foreach (var c in cellsArray)
                {
                    var cell = new CrosswordCellItem
                    {
                        Row = c?["row"]?.GetValue<int>() ?? 0,
                        Column = c?["column"]?.GetValue<int>() ?? 0,
                        IsEmpty = c?["isEmpty"]?.GetValue<bool>() ?? true,
                        Number = c?["number"]?.ToString() ?? string.Empty
                    };

                    cell.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(CrosswordCellItem.UserInput))
                        {
                            SyncCrosswordAnswer();
                        }
                    };

                    CrosswordCells.Add(cell);
                }
            }
        }
        catch (System.Text.Json.JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Помилка парсингу кросворду: {ex.Message}");
        }
    }

    private void SyncCrosswordAnswer()
    {
        var answers = new System.Text.Json.Nodes.JsonObject();
        foreach (var cell in CrosswordCells.Where(c => !c.IsEmpty && !string.IsNullOrWhiteSpace(c.UserInput)))
        {
            // Використовуємо ToUpperInvariant(), щоб уникнути багів з локалями (наприклад, турецьким 'i')
            answers[$"{cell.Row}_{cell.Column}"] = cell.UserInput.Trim().ToUpperInvariant();
        }
        UserAnswer = answers.ToJsonString();
    }

    private void ParseOptionsFromJson(string json)
    {
        try
        {
            var node = System.Text.Json.Nodes.JsonNode.Parse(json);

            var correctNode = node?["correctAnswer"];
            if (correctNode is System.Text.Json.Nodes.JsonArray)
            {
                IsMultipleChoice = true;
            }
            else
            {
                var correctAnswerStr = correctNode?.ToString();
                IsMultipleChoice = !string.IsNullOrWhiteSpace(correctAnswerStr) && correctAnswerStr.Contains(',', StringComparison.Ordinal);
            }

            if (node?["options"] is not System.Text.Json.Nodes.JsonArray optArray) return;

            // Використовуємо LINQ Where для фільтрації замість if у циклі
            var validOptions = optArray
                .Select(opt => opt?.ToString())
                .Where(text => !string.IsNullOrWhiteSpace(text));

            foreach (var text in validOptions)
            {
                Options.Add(new QuizOptionItem(text!));
            }
        }
        catch (System.Text.Json.JsonException ex)
        {
            // Заповнюємо блок catch для аналізатора та полегшення відлагодження
            System.Diagnostics.Debug.WriteLine($"Помилка парсингу JSON для питання {Question.QuestionId}: {ex.Message}");
        }
    }

    private void EnforceSingleSelectionRule()
    {
        foreach (var opt in Options)
        {
            opt.PropertyChanged += (sender, e) =>
            {
                if (!string.Equals(e.PropertyName, nameof(QuizOptionItem.IsSelected), StringComparison.Ordinal) || !opt.IsSelected)
                    return;

                foreach (var other in Options.Where(o => o != opt))
                {
                    other.IsSelected = false;
                }
            };
        }
    }

    public void SyncAnswerFromOptions()
    {
        if (HasOptions)
        {
            // Витягуємо лише маркер варіанту (наприклад "а)") з повного тексту "а) Текст відповіді"
            var selectedKeys = Options
                .Where(o => o.IsSelected)
                .Select(o =>
                {
                    var spaceIndex = o.Text.IndexOf(' ', StringComparison.Ordinal);
                    return spaceIndex > 0 ? o.Text[..spaceIndex] : o.Text;
                });

            UserAnswer = string.Join(", ", selectedKeys);
        }
    }
}

public sealed partial class TakeQuizViewModel(
    IQuizQuestionsApi questionsApi,
    IQuizSubmissionsApi submissionsApi) : AppViewModelBase
{
    [ObservableProperty]
    public partial Guid AssignmentId { get; set; }

    [ObservableProperty]
    public partial Guid QuizId { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<QuestionExecutionItem> Questions { get; set; } = [];

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsSubmitted { get; set; }

    [ObservableProperty]
    public partial int FinalScore { get; set; }

    [ObservableProperty]
    public partial int MaxScore { get; set; }

    /// <summary>
    /// Викликається під час переходу на екран проходження тесту.
    /// </summary>
    public Task InitializeAsync(Guid assignmentId, Guid quizId)
    {
        AssignmentId = assignmentId;
        QuizId = quizId;
        IsSubmitted = false;
        Questions.Clear();

        return LoadQuizQuestionsAsync(CancellationToken.None);
    }

    [RelayCommand]
    private async Task LoadQuizQuestionsAsync(CancellationToken ct)
    {
        if (QuizId == Guid.Empty)
        {
            ErrorMessage = "Системна помилка: Ідентифікатор тесту відсутній.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await questionsApi.GetQuestionsAsync(QuizId, 1, 100, ct).ConfigureAwait(true);
            HandleQuestionsResponse(response);
        }
        catch (ApiException ex)
        {
            ErrorMessage = $"Помилка API при завантаженні: {ex.StatusCode}";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Помилка мережі: {ex.Message}";
        }
        catch (System.Text.Json.JsonException ex)
        {
            ErrorMessage = $"Помилка структури даних (десеріалізація): {ex.Message}";
        }
        catch (OperationCanceledException)
        {
            ErrorMessage = "Завантаження скасовано.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void HandleQuestionsResponse(IApiResponse<SchoolJournal.Contracts.Common.PagedResponse<QuizQuestionResponse>> response)
    {
        if (response.IsSuccessStatusCode && response.Content is not null)
        {
            Questions.Clear();
            foreach (var item in response.Content.Items ?? [])
            {
                Questions.Add(new QuestionExecutionItem(item));
            }

            if (Questions.Count == 0)
            {
                ErrorMessage = "Тест порожній. Для цього тесту ще не додано жодного питання.";
            }

            return;
        }

        ErrorMessage = $"Сервер відхилив запит: {ExtractErrorDetail(response)}";
    }

    private static string ExtractErrorDetail(IApiResponse<SchoolJournal.Contracts.Common.PagedResponse<QuizQuestionResponse>> response)
    {
        string defaultError = $"Код {response.StatusCode}";

        if (response.Error is not ApiException apiEx || string.IsNullOrWhiteSpace(apiEx.Content))
        {
            return defaultError;
        }

        try
        {
            var problem = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonObject>(apiEx.Content);
            return problem?["title"]?.ToString() ?? defaultError;
        }
        catch (System.Text.Json.JsonException)
        {
            return defaultError;
        }
    }

    [RelayCommand]
    private async Task SubmitQuizAsync(CancellationToken ct)
    {
        if (Questions.Count == 0)
        {
            ErrorMessage = "Тест порожній, немає чого відправляти.";
            return;
        }

        await ExecuteLockedAsync(async () =>
        {
            IsLoading = true;
            ErrorMessage = null;

            try
            {               
                foreach (var q in Questions)
                {
                    q.SyncAnswerFromOptions();
                }

                // Захист від null: якщо учень залишив пусте поле, передаємо порожній рядок
                var answers = Questions.Select(q => new QuizAnswerDto(
                    q.Question.QuestionId,
                    q.UserAnswer ?? string.Empty
                )).ToList();

                var request = new SubmitQuizRequest(AssignmentId, answers);

                var response = await submissionsApi.SubmitQuizAsync(request, ct).ConfigureAwait(true);

                if (response.IsSuccessStatusCode && response.Content is not null)
                {
                    IsSubmitted = true;
                    FinalScore = response.Content.Score;
                    MaxScore = response.Content.MaxScore;

                    // Тригер для 3D-моделі Луні через систему подієвих тригерів
                    if (response.Content.EarnedCoins > 0)
                    {
                        WeakReferenceMessenger.Default.Send(new LuniAnimationMessage(response.Content.EarnedCoins));
                    }
                }
                else
                {
                    // FAIL FAST: Дістаємо точну причину відмови сервера
                    ErrorMessage = await ParseSubmissionErrorAsync(response.Error as ApiException, response.StatusCode ?? System.Net.HttpStatusCode.BadRequest).ConfigureAwait(true);
                }
            }
            catch (ApiException ex)
            {
                ErrorMessage = $"Помилка API при здачі: {ex.StatusCode}";
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = $"Помилка мережі: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }).ConfigureAwait(true);
    }

    private static async Task<string> ParseSubmissionErrorAsync(ApiException? apiException, System.Net.HttpStatusCode statusCode)
    {
        string defaultError = $"Відмова сервера (HTTP {statusCode})";
        if (apiException is null || string.IsNullOrWhiteSpace(apiException.Content))
        {
            return defaultError;
        }

        try
        {
            var problem = await apiException.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(false);
            string title = problem?["title"]?.ToString() ?? defaultError;

            // Якщо є деталі валідації (FluentValidation) — дістаємо їх
            if (problem?["errors"] is System.Text.Json.Nodes.JsonObject errorsObj)
            {
                var firstError = errorsObj.FirstOrDefault();
                if (firstError.Value is System.Text.Json.Nodes.JsonArray arr && arr.Count > 0)
                {
                    return $"{title} Деталі: {arr[0]}";
                }
            }

            return title;
        }
        catch (System.Text.Json.JsonException)
        {
            return defaultError;
        }
    }
}