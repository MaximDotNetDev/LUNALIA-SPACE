using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Contracts.DTOs.Operations.QuizQuestions;
using System.Collections.ObjectModel;

namespace SchoolJournal.Client.Core.Features.Operations.QuizQuestions;

// Новий клас для відображення слів кросворду (Read-Only) в редакторі
public sealed partial class EditableCrosswordWord : ObservableObject
{
    [ObservableProperty] public partial int Number { get; set; }
    [ObservableProperty] public partial string Answer { get; set; } = string.Empty;
    [ObservableProperty] public partial string Clue { get; set; } = string.Empty;
}

// Новий клас для відображення одного варіанту відповіді в редакторі
public sealed partial class EditableQuizOption : ObservableObject
{
    [ObservableProperty]
    public partial string Text { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsCorrect { get; set; }
}

public sealed partial class QuizQuestionsViewModel(IQuizQuestionsApi api) : ObservableObject
{
    private static readonly System.Text.Json.JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
    };

    [ObservableProperty]
    public partial ObservableCollection<EditableQuizOption> FormOptions { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<EditableCrosswordWord> FormCrosswordWords { get; set; } = [];

    [ObservableProperty]
    public partial Guid CurrentQuizId { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<QuizQuestionResponse> Questions { get; set; } = [];

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsFormOpen { get; set; }

    [ObservableProperty]
    public partial bool IsEditing { get; set; }

    [ObservableProperty]
    public partial string FormTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial Guid? FormQuestionId { get; set; }

    [ObservableProperty]
    public partial string FormQuestionText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int FormQuestionType { get; set; }

    [ObservableProperty]
    public partial string FormContentJson { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int FormPoints { get; set; } = 1;

    [ObservableProperty]
    public partial bool FormIsTrue { get; set; }

    [ObservableProperty]
    public partial string FormExplanation { get; set; } = string.Empty;

    private string? _formRowVersion;

    [RelayCommand]
    private void AddOption()
    {
        FormOptions.Add(new EditableQuizOption { Text = "новий) Варіант", IsCorrect = false });
    }

    [RelayCommand]
    private void RemoveOption(EditableQuizOption option)
    {
        FormOptions.Remove(option);
    }

    private void ParseContentJsonToOptions(string json)
    {
        FormOptions.Clear();
        if (string.IsNullOrWhiteSpace(json)) return;

        try
        {
            var node = System.Text.Json.Nodes.JsonNode.Parse(json);
            var correctAnswerStr = node?["correctAnswer"]?.ToString() ?? string.Empty;

            var correctKeys = correctAnswerStr
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            // Використовуємо Pattern Matching (is) замість as + null перевірки
            if (node?["options"] is System.Text.Json.Nodes.JsonArray optionsArray)
            {
                foreach (var opt in optionsArray)
                {
                    var text = opt?.ToString();
                    if (string.IsNullOrWhiteSpace(text)) continue;

                    // Відрізаємо літеру для перевірки (наприклад "а)")
                    var spaceIndex = text.IndexOf(' ', StringComparison.Ordinal);
                    var key = spaceIndex > 0 ? text[..spaceIndex] : text;

                    bool isCorrect = correctKeys.Contains(key, StringComparer.OrdinalIgnoreCase);
                    FormOptions.Add(new EditableQuizOption { Text = text, IsCorrect = isCorrect });
                }
            }
        }
        catch (System.Text.Json.JsonException ex)
        {
            // Уникаємо "тихого" проковтування помилок для легшого дебагу
            System.Diagnostics.Debug.WriteLine($"Помилка парсингу JSON: {ex.Message}");
        }
    }

    private string BuildContentJsonFromOptions()
    {
        var optionsArray = new System.Text.Json.Nodes.JsonArray();
        var correctKeys = new List<string>();

        foreach (var opt in FormOptions)
        {
            if (string.IsNullOrWhiteSpace(opt.Text)) continue;

            optionsArray.Add(opt.Text);

            if (opt.IsCorrect)
            {
                var spaceIndex = opt.Text.IndexOf(' ', StringComparison.Ordinal);
                var key = spaceIndex > 0 ? opt.Text[..spaceIndex] : opt.Text;
                correctKeys.Add(key);
            }
        }

        var obj = new System.Text.Json.Nodes.JsonObject
        {
            ["options"] = optionsArray,
            ["correctAnswer"] = string.Join(", ", correctKeys)
        };

        return obj.ToJsonString();
    }

    private void UpdateCrosswordCluesInJson()
    {
        if (string.IsNullOrWhiteSpace(FormContentJson)) return;

        try
        {
            var node = System.Text.Json.Nodes.JsonNode.Parse(FormContentJson);
            if (node?["words"] is System.Text.Json.Nodes.JsonArray wordsArray)
            {
                foreach (var w in wordsArray)
                {
                    var num = w?["number"]?.GetValue<int>();
                    var formWord = FormCrosswordWords.FirstOrDefault(x => x.Number == num);

                    if (formWord != null && w != null)
                    {
                        // Оновлюємо тільки поле clue, не чіпаючи координати
                        w["clue"] = formWord.Clue;
                    }
                }
            }
            FormContentJson = node?.ToJsonString() ?? FormContentJson;
        }
        catch (System.Text.Json.JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Помилка оновлення JSON кросворду: {ex.Message}");
        }
    }

    public async Task InitializeAsync(Guid quizId)
    {
        CurrentQuizId = quizId;
        await LoadQuestionsAsync(CancellationToken.None).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task LoadQuestionsAsync(CancellationToken ct)
    {
        if (CurrentQuizId == Guid.Empty) return;

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await api.GetQuestionsAsync(CurrentQuizId, 1, 100, ct).ConfigureAwait(true);

            // 1. Безпечна перевірка на null
            if (response != null && response.IsSuccessStatusCode && response.Content?.Items != null)
            {
                Questions = new ObservableCollection<QuizQuestionResponse>(response.Content.Items);
            }
            else
            {
                // Отримання опису помилки без ризику NullReference
                string errorTitle = "Невідома помилка.";
                if (response?.Error is ApiException apiEx)
                {
                    try
                    {
                        var problem = await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true);
                        errorTitle = problem?["title"]?.ToString() ?? errorTitle;
                    }
                    catch (System.Text.Json.JsonException) { /* Ігноруємо помилки парсингу JSON */ }
                }
                ErrorMessage = $"Помилка завантаження: {errorTitle}";
            }
        }
        // 2. Ловимо специфічні винятки замість загального Exception
        catch (ApiException ex)
        {
            ErrorMessage = $"Помилка сервера (API): {ex.StatusCode}";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Помилка мережі: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void OpenCreateForm()
    {
        IsEditing = false;
        FormTitle = "Додавання питання";
        FormQuestionId = null;
        FormQuestionText = string.Empty;
        FormQuestionType = 0;
        FormPoints = 1;
        FormIsTrue = true;
        FormExplanation = string.Empty;
        _formRowVersion = null;
        ErrorMessage = null;

        FormOptions.Clear();
        FormOptions.Add(new EditableQuizOption { Text = "а) Варіант 1", IsCorrect = true });
        FormOptions.Add(new EditableQuizOption { Text = "б) Варіант 2", IsCorrect = false });
        FormOptions.Add(new EditableQuizOption { Text = "в) Варіант 3", IsCorrect = false });
        FormOptions.Add(new EditableQuizOption { Text = "г) Варіант 4", IsCorrect = false });

        FormCrosswordWords.Clear();

        IsFormOpen = true;
    }

    [RelayCommand]
    private async Task OpenEditFormAsync(QuizQuestionResponse question, CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await api.GetQuestionByIdAsync(question.QuestionId, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                var fresh = response.Content;
                IsEditing = true;
                FormTitle = "Редагування питання";
                FormQuestionId = fresh.QuestionId;
                FormQuestionText = fresh.QuestionText;
                FormQuestionType = fresh.QuestionType;
                FormContentJson = fresh.ContentJson; 
                FormPoints = fresh.Points;

                FormCrosswordWords.Clear();
                if (fresh.QuestionType == 9 || fresh.QuestionType == 10)
                {
                    // Використовуємо цей самий метод, бо структура масиву "words" (Answer, Clue) у кросворда і філворда ідентична
                    ParseCrosswordJson(fresh.ContentJson);
                }
                else if (fresh.QuestionType == 4)
                {
                    ParseTrueFalseJson(fresh.ContentJson);
                }
                else if (fresh.QuestionType == 5)
                {
                    ParseOddOneOutJson(fresh.ContentJson);
                }
                else
                {
                    ParseContentJsonToOptions(fresh.ContentJson);
                }

                _formRowVersion = fresh.RowVersionBase64;
                IsFormOpen = true;
            }
            else
            {
                ErrorMessage = "Не вдалося завантажити актуальні дані питання.";
            }
        }
        catch (ApiException ex)
        {
            ErrorMessage = $"Помилка сервера при отриманні картки питання: {ex.StatusCode}";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Помилка зв'язку при отриманні картки питання: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ParseTrueFalseJson(string json)
    {
        try
        {
            var node = System.Text.Json.Nodes.JsonNode.Parse(json);
            FormIsTrue = node?["isTrue"]?.GetValue<bool>() ?? true;
            FormExplanation = node?["explanation"]?.ToString() ?? string.Empty;
        }
        catch (System.Text.Json.JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Помилка парсингу JSON (Так/Ні): {ex.Message}");
        }
    }

    private void ParseOddOneOutJson(string json)
    {
        FormOptions.Clear();
        try
        {
            var node = System.Text.Json.Nodes.JsonNode.Parse(json);
            var oddOneOutStr = node?["oddOneOut"]?.ToString() ?? string.Empty;
            FormExplanation = node?["explanation"]?.ToString() ?? string.Empty;

            if (node?["options"] is System.Text.Json.Nodes.JsonArray optionsArray)
            {
                foreach (var opt in optionsArray)
                {
                    var text = opt?.ToString();
                    if (string.IsNullOrWhiteSpace(text)) continue;
                    // Відмічаємо IsCorrect = true, якщо цей термін і є "Зайвим"
                    bool isOdd = string.Equals(text, oddOneOutStr, StringComparison.OrdinalIgnoreCase);
                    FormOptions.Add(new EditableQuizOption { Text = text, IsCorrect = isOdd });
                }
            }
        }
        catch (System.Text.Json.JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Помилка парсингу JSON (Зайве): {ex.Message}");
        }
    }

    private void ParseCrosswordJson(string json)
    {
        try
        {
            var node = System.Text.Json.Nodes.JsonNode.Parse(json);
            if (node?["words"] is System.Text.Json.Nodes.JsonArray wordsArray)
            {
                foreach (var w in wordsArray)
                {
                    FormCrosswordWords.Add(new EditableCrosswordWord
                    {
                        Number = w?["number"]?.GetValue<int>() ?? 0,
                        Answer = w?["answer"]?.ToString() ?? string.Empty,
                        Clue = w?["clue"]?.ToString() ?? string.Empty
                    });
                }
            }
        }
        catch (System.Text.Json.JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Помилка парсингу JSON кросворду: {ex.Message}");
        }
    }

    [RelayCommand]
    private void CloseForm()
    {
        IsFormOpen = false;
        ErrorMessage = null;
    }

    [RelayCommand]
    private async Task SaveFormAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(FormQuestionText))
        {
            ErrorMessage = "Заповніть текст питання.";
            return;
        }

        // Розподіляємо логіку валідації та збереження JSON залежно від типу
        if (FormQuestionType == 9 || FormQuestionType == 10)
        {
            // Оновлює лише текст підказок, не ламаючи координати сітки та літери філворду
            UpdateCrosswordCluesInJson();
        }
        else if (FormQuestionType == 4)
        {
            if (string.IsNullOrWhiteSpace(FormExplanation))
            {
                ErrorMessage = "Для формату 'Так чи ні' пояснення є обов'язковим.";
                return;
            }
            var obj = new { isTrue = FormIsTrue, explanation = FormExplanation };
            FormContentJson = System.Text.Json.JsonSerializer.Serialize(obj, _jsonSerializerOptions);
        }
        else if (FormQuestionType == 5)
        {
            if (FormOptions.Count != 4)
            {
                ErrorMessage = "Для формату 'Знайди зайве' потрібно рівно 4 варіанти.";
                return;
            }
            var oddOption = FormOptions.FirstOrDefault(o => o.IsCorrect);
            if (oddOption == null)
            {
                ErrorMessage = "Позначте галочкою варіант, який є зайвим.";
                return;
            }
            if (string.IsNullOrWhiteSpace(FormExplanation))
            {
                ErrorMessage = "Пояснення (чому термін зайвий) є обов'язковим.";
                return;
            }
            var obj = new { options = FormOptions.Select(o => o.Text).ToList(), oddOneOut = oddOption.Text, explanation = FormExplanation };
            FormContentJson = System.Text.Json.JsonSerializer.Serialize(obj, _jsonSerializerOptions);
        }
        else
        {
            // Для звичайних питань (Тип 0 та 1)
            if (FormOptions.Count == 0)
            {
                ErrorMessage = "Додайте хоча б один варіант відповіді.";
                return;
            }
            if (!FormOptions.Any(o => o.IsCorrect))
            {
                ErrorMessage = "Виберіть хоча б один правильний варіант відповіді.";
                return;
            }
            FormContentJson = BuildContentJsonFromOptions();
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            IApiResponse response;

            if (IsEditing && FormQuestionId.HasValue && _formRowVersion is not null)
            {
                var request = new UpdateQuizQuestionRequest(FormQuestionText, FormQuestionType, FormContentJson, FormPoints, _formRowVersion);
                response = await api.UpdateQuestionAsync(FormQuestionId.Value, request, ct).ConfigureAwait(true);
            }
            else
            {
                var request = new CreateQuizQuestionRequest(FormQuestionText, FormQuestionType, FormContentJson, FormPoints);
                response = await api.CreateQuestionAsync(CurrentQuizId, request, ct).ConfigureAwait(true);
            }

            if (response.IsSuccessStatusCode)
            {
                IsFormOpen = false;
                await LoadQuestionsAsync(ct).ConfigureAwait(true);
            }
            else if (response.Error is ApiException apiEx)
            {
                var problem = await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true);
                ErrorMessage = problem?["title"]?.ToString() ?? "Помилка збереження даних.";
            }
        }
        catch (ApiException ex)
        {
            ErrorMessage = $"Помилка сервера при збереженні форми: {ex.StatusCode}";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Мережева помилка при збереженні форми: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteQuestionAsync(QuizQuestionResponse question, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var request = new DeleteQuizQuestionRequest(question.RowVersionBase64);
            var response = await api.DeleteQuestionAsync(question.QuestionId, request, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                Questions.Remove(question);
            }
            else if (response.Error is not null)
            {
var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                ErrorMessage = problem?["title"]?.ToString() ?? "Помилка видалення.";
            }
        }
        catch (ApiException ex)
        {
            ErrorMessage = $"Помилка API при видаленні питання: {ex.StatusCode}";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Помилка мережі при видаленні питання: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task MoveUpAsync(QuizQuestionResponse question, CancellationToken ct)
    {
        int index = Questions.IndexOf(question);
        if (index <= 0) return;

        Questions.Move(index, index - 1);
        await SyncOrderAsync(ct).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task MoveDownAsync(QuizQuestionResponse question, CancellationToken ct)
    {
        int index = Questions.IndexOf(question);
        if (index < 0 || index >= Questions.Count - 1) return;

        Questions.Move(index, index + 1);
        await SyncOrderAsync(ct).ConfigureAwait(true);
    }

    private async Task SyncOrderAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var items = Questions.Select((q, idx) => new ReorderItem(q.QuestionId, idx)).ToList();
            var request = new ReorderQuizQuestionsRequest(items);

            var response = await api.ReorderQuestionsAsync(CurrentQuizId, request, ct).ConfigureAwait(true);
            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Не вдалося зберегти новий порядок питань на сервері.";
                await LoadQuestionsAsync(ct).ConfigureAwait(true); 
            }
        }
        catch (ApiException ex)
        {
            ErrorMessage = $"Помилка API при реордерингу: {ex.StatusCode}";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Помилка мережі при реордерингу: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}