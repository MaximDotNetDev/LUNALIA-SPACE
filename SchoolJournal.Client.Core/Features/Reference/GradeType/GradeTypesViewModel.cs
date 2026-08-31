using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Contracts.DTOs.Reference.GradeTypes;
using SchoolJournal.Contracts.Enums.Identity;
using System.Collections.ObjectModel;

namespace SchoolJournal.Client.Core.Features.Reference.GradeType;

public sealed partial class GradeTypesViewModel : ObservableObject
{
    private readonly IGradeTypeApi _gradeTypeApi;
    private readonly IIdentityService _identityService;

    public GradeTypesViewModel(IGradeTypeApi gradeTypeApi, IIdentityService identityService)
    {
        _gradeTypeApi = gradeTypeApi;
        _identityService = identityService;

        IsAdminOrDirector = _identityService.IsInRole(RoleType.Admin, RoleType.Director);

        _ = InitializeCommand.ExecuteAsync(null);
    }

    [ObservableProperty]
    private ObservableCollection<GradeTypeResponse> _activeGradeTypes = [];

    [ObservableProperty]
    private ObservableCollection<GradeTypeResponse> _archivedGradeTypes = [];

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
    private Guid? _formGradeTypeId;

    [ObservableProperty]
    private string _formTypeName = string.Empty;

    [RelayCommand]
    private async Task InitializeAsync(CancellationToken ct)
    {
        await Task.WhenAll(
            LoadActiveAsync(ct),
            LoadArchiveAsync(ct)
        ).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task LoadActiveAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _gradeTypeApi.GetActiveGradeTypesAsync(ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                ActiveGradeTypes = new ObservableCollection<GradeTypeResponse>(response.Content);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження активних типів оцінок: {ex.Message}";
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
            var response = await _gradeTypeApi.GetDeletedGradeTypesArchiveAsync(1, 100, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                ArchivedGradeTypes = new ObservableCollection<GradeTypeResponse>(response.Content.Items);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = "Не вдалося завантажити архів типів оцінок.";
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
        FormTitle = "Створення типу оцінки";
        FormGradeTypeId = null;
        FormTypeName = string.Empty;
        ErrorMessage = null;
        IsFormOpen = true;
    }

    [RelayCommand]
    private async Task OpenEditFormAsync(GradeTypeResponse gradeType, CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _gradeTypeApi.GetGradeTypeByIdAsync(gradeType.GradeTypeId, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                IsEditing = true;
                FormTitle = "Редагування типу оцінки";
                FormGradeTypeId = response.Content.GradeTypeId;
                FormTypeName = response.Content.TypeName;
                IsFormOpen = true;
            }
            else
            {
                ErrorMessage = "Не вдалося завантажити актуальні дані для редагування.";
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження: {ex.Message}";
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
        if (string.IsNullOrWhiteSpace(FormTypeName))
        {
            ErrorMessage = "Будь ласка, заповніть назву.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            IApiResponse response;

            if (IsEditing && FormGradeTypeId.HasValue)
            {
                var request = new UpdateGradeTypeRequest(FormTypeName);
                response = await _gradeTypeApi.UpdateGradeTypeAsync(FormGradeTypeId.Value, request, ct).ConfigureAwait(true);
            }
            else
            {
                var request = new CreateGradeTypeRequest(FormTypeName);
                response = await _gradeTypeApi.CreateGradeTypeAsync(request, ct).ConfigureAwait(true);
            }

            if (response.IsSuccessStatusCode)
            {
                IsFormOpen = false;
                await LoadActiveAsync(ct).ConfigureAwait(true);
            }
            else if (response.Error is not null)
            {
var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                var serverMessage = problem?["title"]?.ToString();

                ErrorMessage = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.BadRequest => serverMessage ?? "Некоректні дані.",
                    System.Net.HttpStatusCode.Conflict => serverMessage ?? "Тип оцінки з такою назвою вже існує.",
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
    private async Task DeleteAsync(GradeTypeResponse gradeType, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var response = await _gradeTypeApi.DeleteGradeTypeAsync(gradeType.GradeTypeId, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                ActiveGradeTypes.Remove(gradeType);
                await LoadArchiveAsync(ct).ConfigureAwait(true);
            }
            else if (response.Error is not null)
            {
var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                var serverMessage = problem?["title"]?.ToString();
                ErrorMessage = serverMessage ?? "Помилка видалення.";
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка системи: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RestoreAsync(GradeTypeResponse gradeType, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var response = await _gradeTypeApi.RestoreGradeTypeAsync(gradeType.GradeTypeId, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                ArchivedGradeTypes.Remove(gradeType);
                await LoadActiveAsync(ct).ConfigureAwait(true);
            }
            else if (response.Error is not null)
            {
var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                var serverMessage = problem?["title"]?.ToString();
                ErrorMessage = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Conflict => serverMessage ?? "Конфлікт при відновленні.",
                    _ => serverMessage ?? "Помилка відновлення."
                };
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка системи: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}