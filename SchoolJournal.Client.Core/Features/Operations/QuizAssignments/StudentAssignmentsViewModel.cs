using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SchoolJournal.Client.Core.Features.Core.Students;
using SchoolJournal.Client.Core.Features.Testing;
using SchoolJournal.Contracts.DTOs.Operations.QuizAssignments;
using SchoolJournal.Client.Core.Common.Auth;
using System.Collections.ObjectModel;
using System.Text.Json.Nodes;
using Refit;

namespace SchoolJournal.Client.Core.Features.Operations.QuizAssignments;

public sealed partial class StudentAssignmentsViewModel : ObservableObject
{
    private readonly IQuizAssignmentsApi _quizAssignmentsApi;
    private readonly IStudentApi _studentApi;
    private readonly ITokenStorageService _tokenStorageService;

    public StudentAssignmentsViewModel(
        IQuizAssignmentsApi quizAssignmentsApi,
        IStudentApi studentApi,
        ITokenStorageService tokenStorageService)
    {
        _quizAssignmentsApi = quizAssignmentsApi;
        _studentApi = studentApi;
        _tokenStorageService = tokenStorageService;

        _ = InitializeStudentDataAsync();
    }

    [ObservableProperty] public partial ObservableCollection<QuizAssignmentResponse> Assignments { get; set; } = [];
    [ObservableProperty] public partial bool IsLoading { get; set; }
    [ObservableProperty] public partial string? ErrorMessage { get; set; }

    private async Task InitializeStudentDataAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        Assignments.Clear();
        try
        {
            var (accessToken, _) = await _tokenStorageService.GetTokensAsync(CancellationToken.None).ConfigureAwait(true);
            var userId = ExtractUserIdFromJwt(accessToken);

            if (userId == Guid.Empty)
            {
                ErrorMessage = "Помилка авторизації: неможливо визначити ваш акаунт.";
                return;
            }

            var studentResponse = await _studentApi.GetStudentByUserIdAsync(userId, CancellationToken.None).ConfigureAwait(true);

            if (studentResponse.IsSuccessStatusCode && studentResponse.Content is not null)
            {
                await LoadAssignmentsAsync(studentResponse.Content.ClassId).ConfigureAwait(true);
            }
            else
            {
                string errorDetail = studentResponse.StatusCode?.ToString() ?? "Unknown Error";
                if (studentResponse.Error is ApiException error)
                {
                    try { var problem = await error.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true); errorDetail = problem?["title"]?.ToString() ?? errorDetail; }
                    catch (System.Text.Json.JsonException) { /* Ігноруємо помилки парсингу */ }
                }
                ErrorMessage = $"Помилка завантаження профілю: {errorDetail}";
            }
        }
        catch (ApiException ex)
        {
            ErrorMessage = $"Помилка API: {ex.StatusCode}";
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

    private async Task LoadAssignmentsAsync(Guid classId)
    {
        try
        {
            var response = await _quizAssignmentsApi.GetActiveQuizAssignmentsByClassIdAsync(classId, CancellationToken.None).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                Assignments.Clear();
                foreach (var assignment in response.Content)
                {
                    Assignments.Add(assignment);
                }

                if (Assignments.Count == 0) ErrorMessage = "Ура! Для вашого класу поки немає призначених тестів 🎉";
            }
            else
            {
                string errorDetail = response.StatusCode?.ToString() ?? "Unknown Error";
                if (response.Error is ApiException error)
                {
                    try { var problem = await error.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true); errorDetail = problem?["title"]?.ToString() ?? errorDetail; }
                    catch (System.Text.Json.JsonException) { /* Ігноруємо помилки парсингу */ }
                }
                ErrorMessage = $"Бекенд відмовив у доступі: {errorDetail}";
            }
        }
        catch (ApiException ex)
        {
            ErrorMessage = $"Помилка API: {ex.StatusCode}";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Помилка мережі: {ex.Message}";
        }
    }

    [RelayCommand]
    private static void StartQuiz(QuizAssignmentResponse? assignment)
    {
        if (assignment is null) return;
        WeakReferenceMessenger.Default.Send(new OpenTakeQuizMessage(assignment.AssignmentId, assignment.QuizId));
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