using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Client.Core.Features.Identity.Common;
using SchoolJournal.Contracts.DTOs.Reference.Semesters;
using SchoolJournal.Contracts.Enums.Identity;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

namespace SchoolJournal.Client.Core.Features.Reference.Semester;

public sealed partial class SemestersViewModel : ObservableObject
{
    private readonly ISemesterApi _semesterApi;
    private readonly IIdentityService _identityService;

    public SemestersViewModel(ISemesterApi semesterApi, IIdentityService identityService)
    {
        _semesterApi = semesterApi;
        _identityService = identityService;

        IsAdminOrDirector = _identityService.IsInRole(RoleType.Admin, RoleType.Director);

        _ = InitializeCommand.ExecuteAsync(null);
    }

    [ObservableProperty]
    private ObservableCollection<SemesterResponse> _activeSemesters = [];

    [ObservableProperty]
    private ObservableCollection<SemesterResponse> _archivedSemesters = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isAdminOrDirector;

    [ObservableProperty]
    private bool _isFormOpen;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _formTitle = string.Empty;

    [ObservableProperty]
    private Guid? _formSemesterId;

    [ObservableProperty]
    private string _formSemesterName = string.Empty;

    [ObservableProperty]
    private DateTime? _formStartDate;

    [ObservableProperty]
    private DateTime? _formEndDate;

    private string? _formRowVersion;

    [RelayCommand]
    private async Task InitializeAsync(CancellationToken ct)
    {
        await Task.WhenAll(
            LoadActiveSemestersAsync(ct),
            LoadArchiveAsync(ct)
        ).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task LoadActiveSemestersAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _semesterApi.GetActiveSemestersAsync(1, 100, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                ActiveSemesters = new ObservableCollection<SemesterResponse>(response.Content.Items);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження активних семестрів: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadArchiveAsync(CancellationToken ct)
    {
        if (!IsAdminOrDirector) return;

        IsLoading = true;

        try
        {
            var response = await _semesterApi.GetDeletedSemestersAsync(1, 50, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                ArchivedSemesters = new ObservableCollection<SemesterResponse>(response.Content.Items);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = "Не вдалося завантажити архів семестрів. Можливо, проблема з мережею або правами доступу.";
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
        FormTitle = "Створення семестру";
        FormSemesterId = null;
        FormSemesterName = string.Empty;
        FormStartDate = DateTime.Today;
        FormEndDate = DateTime.Today.AddMonths(4);
        _formRowVersion = null;
        ErrorMessage = null;
        IsFormOpen = true;
    }

    [RelayCommand]
    private async Task OpenEditFormAsync(SemesterResponse semester, CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _semesterApi.GetSemesterByIdAsync(semester.SemesterId, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                var freshSemester = response.Content;
                IsEditing = true;
                FormTitle = "Редагування семестру";
                FormSemesterId = freshSemester.SemesterId;
                FormSemesterName = freshSemester.SemesterName;
                FormStartDate = freshSemester.StartDate.LocalDateTime;
                FormEndDate = freshSemester.EndDate.LocalDateTime;
                _formRowVersion = freshSemester.RowVersionBase64; 
                IsFormOpen = true;
            }
            else
            {
                ErrorMessage = "Не вдалося завантажити актуальні дані семестру для редагування.";
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження семестру: {ex.Message}";
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
    private async Task SaveFormAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(FormSemesterName) || FormStartDate is null || FormEndDate is null)
        {
            ErrorMessage = "Будь ласка, заповніть всі поля.";
            return;
        }

        if (FormEndDate <= FormStartDate)
        {
            ErrorMessage = "Дата закінчення має бути пізнішою за дату початку.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var startDateUtc = new DateTimeOffset(DateTime.SpecifyKind(FormStartDate.Value.Date, DateTimeKind.Utc));
            var endDateUtc = new DateTimeOffset(DateTime.SpecifyKind(FormEndDate.Value.Date, DateTimeKind.Utc));

            IApiResponse response;

            if (IsEditing && FormSemesterId.HasValue && _formRowVersion is not null)
            {
                var request = new UpdateSemesterRequest(FormSemesterName, startDateUtc, endDateUtc, _formRowVersion);
                response = await _semesterApi.UpdateSemesterAsync(FormSemesterId.Value, request, ct).ConfigureAwait(true);
            }
            else
            {
                var request = new CreateSemesterRequest(FormSemesterName, startDateUtc, endDateUtc);
                response = await _semesterApi.CreateSemesterAsync(request, ct).ConfigureAwait(true);
            }

            if (response.IsSuccessStatusCode)
            {
                IsFormOpen = false;
                await LoadActiveSemestersAsync(ct).ConfigureAwait(true);
            }
            else if (response.Error is not null) 
            {
var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                var serverMessage = problem?["title"]?.ToString();

                ErrorMessage = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.BadRequest => serverMessage ?? "Некоректні дані семестру.",
                    System.Net.HttpStatusCode.Conflict => serverMessage ?? "Конфлікт даних: дані змінені або вже існують.",
                    _ => $"Помилка API: {response.Error.Message}"
                };
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
    private async Task DeleteSemesterAsync(SemesterResponse semester, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var request = new DeleteSemesterRequest(semester.RowVersionBase64);
            var response = await _semesterApi.DeleteSemesterAsync(semester.SemesterId, request, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                ActiveSemesters.Remove(semester);
                await LoadArchiveAsync(ct).ConfigureAwait(true);
            }
            else if (response.Error is not null)
            {
var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                var serverMessage = problem?["title"]?.ToString();

                ErrorMessage = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Conflict => "Помилка паралельного доступу (RowVersion не збігається).",
                    System.Net.HttpStatusCode.BadRequest => serverMessage ?? "Некоректний запит на видалення.",
                    _ => $"Помилка видалення: {response.Error.Message}"
                };
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
    private async Task RestoreSemesterAsync(SemesterResponse semester, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var request = new RestoreSemesterRequest(semester.RowVersionBase64);
            var response = await _semesterApi.RestoreSemesterAsync(semester.SemesterId, request, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                ArchivedSemesters.Remove(semester);
                await LoadActiveSemestersAsync(ct).ConfigureAwait(true);
            }
            else if (response.Error is not null)
            {
var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                var serverMessage = problem?["title"]?.ToString();

                ErrorMessage = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.NotFound => "Семестр не знайдено в архіві.",
                    System.Net.HttpStatusCode.BadRequest => serverMessage ?? "Некоректний запит на відновлення.",
                    _ => $"Помилка відновлення: {response.Error.Message}"
                };
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
}