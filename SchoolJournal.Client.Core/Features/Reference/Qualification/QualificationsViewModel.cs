using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Contracts.DTOs.Reference.Qualifications;
using SchoolJournal.Contracts.Enums.Identity;
using System.Collections.ObjectModel;

namespace SchoolJournal.Client.Core.Features.Reference.Qualification;

public sealed partial class QualificationsViewModel : ObservableObject
{
    private readonly IQualificationApi _qualificationApi;
    private readonly IIdentityService _identityService;

    public QualificationsViewModel(IQualificationApi qualificationApi, IIdentityService identityService)
    {
        _qualificationApi = qualificationApi;
        _identityService = identityService;

        IsAdminOrDirector = _identityService.IsInRole(RoleType.Admin, RoleType.Director);

        _ = InitializeCommand.ExecuteAsync(null);
    }

    [ObservableProperty]
    private ObservableCollection<QualificationResponse> _activeQualifications = [];

    [ObservableProperty]
    private ObservableCollection<QualificationResponse> _archivedQualifications = [];

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
    private Guid? _formQualificationId;

    [ObservableProperty]
    private string _formQualificationName = string.Empty;

    private string? _formRowVersion;

    [RelayCommand]
    private async Task InitializeAsync(CancellationToken ct)
    {
        await Task.WhenAll(
            LoadActiveQualificationsAsync(ct),
            LoadArchiveAsync(ct)
        ).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task LoadActiveQualificationsAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _qualificationApi.GetActiveQualificationsAsync(1, 100, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                ActiveQualifications = new ObservableCollection<QualificationResponse>(response.Content.Items);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження активних кваліфікацій: {ex.Message}";
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
            var response = await _qualificationApi.GetDeletedQualificationsAsync(1, 50, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                ArchivedQualifications = new ObservableCollection<QualificationResponse>(response.Content.Items);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = "Не вдалося завантажити архів кваліфікацій. Можливо, проблема з мережею або правами доступу.";
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
        FormTitle = "Створення кваліфікації";
        FormQualificationId = null;
        FormQualificationName = string.Empty;
        _formRowVersion = null;
        ErrorMessage = null;
        IsFormOpen = true;
    }

    [RelayCommand]
    private async Task OpenEditFormAsync(QualificationResponse qualification, CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _qualificationApi.GetQualificationByIdAsync(qualification.QualificationId, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                var freshData = response.Content;
                IsEditing = true;
                FormTitle = "Редагування кваліфікації";
                FormQualificationId = freshData.QualificationId;
                FormQualificationName = freshData.QualificationName;
                _formRowVersion = freshData.RowVersionBase64;
                IsFormOpen = true;
            }
            else
            {
                ErrorMessage = "Не вдалося завантажити актуальні дані для редагування.";
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження кваліфікації: {ex.Message}";
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
        if (string.IsNullOrWhiteSpace(FormQualificationName))
        {
            ErrorMessage = "Будь ласка, заповніть назву кваліфікації.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            IApiResponse response;

            if (IsEditing && FormQualificationId.HasValue && _formRowVersion is not null)
            {
                var request = new UpdateQualificationRequest(FormQualificationName, _formRowVersion);
                response = await _qualificationApi.UpdateQualificationAsync(FormQualificationId.Value, request, ct).ConfigureAwait(true);
            }
            else
            {
                var request = new CreateQualificationRequest(FormQualificationName);
                response = await _qualificationApi.CreateQualificationAsync(request, ct).ConfigureAwait(true);
            }

            if (response.IsSuccessStatusCode)
            {
                IsFormOpen = false;
                await LoadActiveQualificationsAsync(ct).ConfigureAwait(true);
            }
            else if (response.Error is not null)
            {
var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                var serverMessage = problem?["title"]?.ToString();

                ErrorMessage = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.BadRequest => serverMessage ?? "Некоректні дані.",
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
    private async Task DeleteQualificationAsync(QualificationResponse qualification, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var request = new DeleteQualificationRequest(qualification.RowVersionBase64);
            var response = await _qualificationApi.DeleteQualificationAsync(qualification.QualificationId, request, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                ActiveQualifications.Remove(qualification);
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
    private async Task RestoreQualificationAsync(QualificationResponse qualification, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var request = new RestoreQualificationRequest(qualification.RowVersionBase64);
            var response = await _qualificationApi.RestoreQualificationAsync(qualification.QualificationId, request, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                ArchivedQualifications.Remove(qualification);
                await LoadActiveQualificationsAsync(ct).ConfigureAwait(true);
            }
            else if (response.Error is not null)
            {
var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                var serverMessage = problem?["title"]?.ToString();

                ErrorMessage = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.NotFound => "Кваліфікацію не знайдено в архіві.",
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