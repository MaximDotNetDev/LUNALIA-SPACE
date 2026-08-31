using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Contracts.DTOs.Reference.PedagogicalTitles;
using SchoolJournal.Contracts.Enums.Identity;
using System.Collections.ObjectModel;

namespace SchoolJournal.Client.Core.Features.Reference.PedagogicalTitle;

public sealed partial class PedagogicalTitlesViewModel : ObservableObject
{
    private readonly IPedagogicalTitleApi _api;
    private readonly IIdentityService _identityService;

    public PedagogicalTitlesViewModel(IPedagogicalTitleApi api, IIdentityService identityService)
    {
        _api = api;
        _identityService = identityService;

        IsAdminOrDirector = _identityService.IsInRole(RoleType.Admin, RoleType.Director);

        _ = InitializeCommand.ExecuteAsync(null);
    }

    [ObservableProperty]
    private ObservableCollection<PedagogicalTitleResponse> _activeTitles = [];

    [ObservableProperty]
    private ObservableCollection<PedagogicalTitleResponse> _archivedTitles = [];

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
    private string _formTitleText = string.Empty;

    [ObservableProperty]
    private Guid? _formTitleId;

    [ObservableProperty]
    private string _formTitleName = string.Empty;

    [RelayCommand]
    private async Task InitializeAsync(CancellationToken ct)
    {
        await Task.WhenAll(
            LoadActiveTitlesAsync(ct),
            LoadArchiveAsync(ct)
        ).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task LoadActiveTitlesAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _api.GetActivePagedAsync(1, 100, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                ActiveTitles = new ObservableCollection<PedagogicalTitleResponse>(response.Content.Items);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження активних звань: {ex.Message}";
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
            var response = await _api.GetDeletedPagedAsync(1, 50, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                ArchivedTitles = new ObservableCollection<PedagogicalTitleResponse>(response.Content.Items);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = "Не вдалося завантажити архів звань.";
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
        FormTitleText = "Створення педагогічного звання";
        FormTitleId = null;
        FormTitleName = string.Empty;
        ErrorMessage = null;
        IsFormOpen = true;
    }

    [RelayCommand]
    private void OpenEditForm(PedagogicalTitleResponse title)
    {
        IsEditing = true;
        FormTitleText = "Редагування педагогічного звання";
        FormTitleId = title.TitleId;
        FormTitleName = title.TitleName;
        ErrorMessage = null;
        IsFormOpen = true;
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
        if (string.IsNullOrWhiteSpace(FormTitleName))
        {
            ErrorMessage = "Будь ласка, вкажіть назву звання.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            IApiResponse response;

            if (IsEditing && FormTitleId.HasValue)
            {
                var request = new UpdatePedagogicalTitleRequest(FormTitleName);
                response = await _api.UpdateAsync(FormTitleId.Value, request, ct).ConfigureAwait(true);
            }
            else
            {
                var request = new CreatePedagogicalTitleRequest(FormTitleName);
                response = await _api.CreateAsync(request, ct).ConfigureAwait(true);
            }

            if (response.IsSuccessStatusCode)
            {
                IsFormOpen = false;
                await LoadActiveTitlesAsync(ct).ConfigureAwait(true);
            }
            else if (response.Error is not null)
            {
var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                var serverMessage = problem?["title"]?.ToString();

                ErrorMessage = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.BadRequest => serverMessage ?? "Некоректні дані.",
                    System.Net.HttpStatusCode.Conflict => serverMessage ?? "Таке звання вже існує.",
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
    private async Task DeleteTitleAsync(PedagogicalTitleResponse title, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var response = await _api.DeleteAsync(title.TitleId, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                ActiveTitles.Remove(title);
                await LoadArchiveAsync(ct).ConfigureAwait(true);
            }
            else if (response.Error is not null)
            {
                ErrorMessage = $"Помилка видалення: {response.Error.Message}";
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
    private async Task RestoreTitleAsync(PedagogicalTitleResponse title, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var response = await _api.RestoreAsync(title.TitleId, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                ArchivedTitles.Remove(title);
                await LoadActiveTitlesAsync(ct).ConfigureAwait(true);
            }
            else if (response.Error is not null)
            {
                ErrorMessage = $"Помилка відновлення: {response.Error.Message}";
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