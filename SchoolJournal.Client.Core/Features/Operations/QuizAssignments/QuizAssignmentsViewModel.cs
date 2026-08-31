using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Contracts.DTOs.Operations.QuizAssignments;
using System.Collections.ObjectModel;
using SchoolJournal.Client.Core.Features.Operations.Quizzes;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;
using CommunityToolkit.Mvvm.Messaging;
using SchoolJournal.Client.Core.Features.Testing;
using SchoolJournal.Client.Core.Features.Operations.QuizSubmissions;
using SchoolJournal.Contracts.DTOs.Operations.QuizSubmissions;

namespace SchoolJournal.Client.Core.Features.Operations.QuizAssignments;


public sealed partial class QuizAssignmentsViewModel : ObservableObject
{
    private const string ProblemTitleKey = "title";

    private readonly IQuizAssignmentsApi _quizAssignmentsApi;
    private readonly IQuizzesApi _quizzesApi;
    private readonly IQuizSubmissionsApi _quizSubmissionsApi;

    public QuizAssignmentsViewModel(
        IQuizAssignmentsApi quizAssignmentsApi,
        IQuizzesApi quizzesApi,
        IQuizSubmissionsApi quizSubmissionsApi)
    {
        _quizAssignmentsApi = quizAssignmentsApi;
        _quizzesApi = quizzesApi;
        _quizSubmissionsApi = quizSubmissionsApi;

        _ = LoadQuizzesAsync(CancellationToken.None);
    }

    [ObservableProperty]
    public partial ObservableCollection<QuizSubmissionResultDto> SubmissionResults { get; set; } = [];

    [ObservableProperty]
    public partial bool IsResultsOpen { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<QuizResponse> AvailableQuizzes { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<QuizAssignmentResponse> Assignments { get; set; } = [];

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial Guid? SearchQuizId { get; set; }

    private async Task LoadQuizzesAsync(CancellationToken ct)
    {
        try
        {
            // Змінюємо розмір сторінки з 1000 на 100, щоб задовольнити ліміти бекенду
            var response = await _quizzesApi.GetQuizzesPagedAsync(null, 1, 100, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                var newQuizzes = response.Content.Items;

                AvailableQuizzes.Clear(); // Очищаємо стару колекцію, не руйнуючи Binding
                foreach (var quiz in newQuizzes)
                {
                    AvailableQuizzes.Add(quiz); // Додаємо по одному, що змушує ComboBox оновитись
                }
            }
            else if (response.Error is not null)
            {
var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                ErrorMessage = problem?[ProblemTitleKey]?.ToString() ?? "Помилка завантаження тестів з сервера.";
            }
        }
        catch (ApiException ex)
        {
            ErrorMessage = $"Помилка API при завантаженні списку тестів: {ex.StatusCode}";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Помилка мережі: {ex.Message}";
        }
    }

    [ObservableProperty]
    public partial bool IsEditFormOpen { get; set; }

    [ObservableProperty]
    public partial Guid EditAssignmentId { get; set; }

    [ObservableProperty]
    public partial DateTime? EditDueDate { get; set; }

    private string? _editRowVersion;

    [RelayCommand]
    private async Task LoadAssignmentsByQuizAsync(CancellationToken ct)
    {
        if (!SearchQuizId.HasValue)
        {
            ErrorMessage = "Будь ласка, оберіть тест зі списку.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _quizAssignmentsApi.GetActiveQuizAssignmentsByQuizIdAsync(SearchQuizId.Value, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                Assignments = new ObservableCollection<QuizAssignmentResponse>(response.Content);
                if (Assignments.Count == 0)
                {
                    ErrorMessage = "Призначень для цього тесту не знайдено.";
                }
            }
            else if (response.Error is not null)
            {
var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                ErrorMessage = problem?[ProblemTitleKey]?.ToString() ?? "Помилка завантаження.";
            }
        }
        catch (ApiException ex)
        {
            ErrorMessage = $"Помилка API при завантаженні журналу: {ex.StatusCode}";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Помилка зв'язку при завантаженні журналу: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void OpenEditForm(QuizAssignmentResponse assignment)
    {
        EditAssignmentId = assignment.AssignmentId;
        EditDueDate = assignment.DueDate?.LocalDateTime;
        _editRowVersion = assignment.RowVersionBase64;
        ErrorMessage = null;
        IsEditFormOpen = true;
    }

    [RelayCommand]
    private void CloseEditForm()
    {
        IsEditFormOpen = false;
        ErrorMessage = null;
    }

    [RelayCommand]
    private async Task SaveEditFormAsync(CancellationToken ct)
    {
        if (_editRowVersion is null) return;

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            DateTimeOffset? newDueDate = EditDueDate.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(EditDueDate.Value, DateTimeKind.Utc))
                : null;

            var request = new UpdateQuizAssignmentDueDateRequest(newDueDate, _editRowVersion);
            var response = await _quizAssignmentsApi.UpdateQuizAssignmentDueDateAsync(EditAssignmentId, request, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                IsEditFormOpen = false;
                await LoadAssignmentsByQuizAsync(ct).ConfigureAwait(true);
            }
            else if (response.Error is not null)
            {
var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                ErrorMessage = problem?[ProblemTitleKey]?.ToString() ?? "Помилка збереження.";
            }
        }
        catch (ApiException ex)
        {
            ErrorMessage = $"Помилка API при оновленні дедлайну: {ex.StatusCode}";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Помилка мережі при оновленні дедлайну: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAssignmentAsync(QuizAssignmentResponse assignment, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var request = new DeleteQuizAssignmentRequest(assignment.RowVersionBase64);
            var response = await _quizAssignmentsApi.DeleteQuizAssignmentAsync(assignment.AssignmentId, request, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                Assignments.Remove(assignment);
            }
            else if (response.Error is not null)
            {
var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                ErrorMessage = problem?[ProblemTitleKey]?.ToString() ?? "Помилка скасування.";
            }
        }
        catch (OperationCanceledException)
        {
            ErrorMessage = "Запит було скасовано.";
        }
        catch (Exception ex)
        {
            if (ex is OutOfMemoryException or StackOverflowException or AccessViolationException) throw;
            ErrorMessage = $"Помилка системи: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void OpenTakeQuiz(QuizAssignmentResponse assignment)
    {
        // Відправляємо повідомлення в TestingViewModel з реальними ID для відкриття екрану учня
        if (SearchQuizId.HasValue)
        {
            WeakReferenceMessenger.Default.Send(new OpenTakeQuizMessage(assignment.AssignmentId, SearchQuizId.Value));
        }
    }

    [RelayCommand]
    private async Task OpenResultsAsync(QuizAssignmentResponse assignment, CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _quizSubmissionsApi.GetAssignmentSubmissionsAsync(assignment.AssignmentId, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                SubmissionResults = new ObservableCollection<QuizSubmissionResultDto>(response.Content);
                IsResultsOpen = true;
            }
            else
            {
                // FAIL FAST: Виводимо максимально точну помилку, яку повернув бекенд (HTTP статус + сирий текст)
                string rawError = (response.Error is ApiException apiEx ? apiEx.Content : null) ?? "Порожнє тіло помилки";
                ErrorMessage = $"Бекенд відхилив запит (HTTP {(int?)response.StatusCode}). Деталі: {rawError}";
            }
        }
        catch (ApiException ex)
        {
            ErrorMessage = $"Помилка Refit API: {(int)ex.StatusCode}. Деталі: {ex.Content}";
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
            ErrorMessage = "Запит було скасовано.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void CloseResults()
    {
        IsResultsOpen = false;
        SubmissionResults.Clear(); // Звільняємо пам'ять
        ErrorMessage = null;
    }
}