using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Client.Core.Features.Identity.Common;
using SchoolJournal.Contracts.DTOs.Core.Parents;
using SchoolJournal.Contracts.Enums.Identity;
using SchoolJournal.Contracts.DTOs.Identity.Register;
using System.Collections.ObjectModel;
using System.Text.Json.Nodes;

namespace SchoolJournal.Client.Core.Features.Core.Parents;

public sealed partial class ParentsViewModel : ObservableObject
{
    private readonly IParentApi _parentApi;
    private readonly IIdentityService _identityService;
    private readonly IIdentityApi _identityApi;

    public ParentsViewModel(IParentApi parentApi, IIdentityService identityService, IIdentityApi identityApi)
    {
        _parentApi = parentApi;
        _identityService = identityService;
        _identityApi = identityApi;
        IsAdminOrDirector = _identityService.IsInRole(RoleType.Admin, RoleType.Director);
        _ = InitializeCommand.ExecuteAsync(null);
    }

    [ObservableProperty] public partial ObservableCollection<ParentResponse> ActiveParents { get; set; } = [];
    [ObservableProperty] public partial bool IsLoading { get; set; }
    [ObservableProperty] public partial string? ErrorMessage { get; set; }
    [ObservableProperty] public partial bool IsAdminOrDirector { get; set; }

    // Form fields
    [ObservableProperty] public partial bool IsFormOpen { get; set; }
    [ObservableProperty] public partial bool IsEditing { get; set; }
    [ObservableProperty] public partial string FormTitle { get; set; } = string.Empty;
    [ObservableProperty] public partial Guid? FormParentId { get; set; }
    [ObservableProperty] public partial string? FormLastName { get; set; }
    [ObservableProperty] public partial string? FormFirstName { get; set; }
    [ObservableProperty] public partial string? FormMiddleName { get; set; }
    [ObservableProperty] public partial string? FormPhone { get; set; }
    private string? _formRowVersion;

    // Account fields
    [ObservableProperty] public partial bool IsAccountFormOpen { get; set; }
    [ObservableProperty] public partial string AccountLogin { get; set; } = string.Empty;
    [ObservableProperty] public partial string AccountPassword { get; set; } = string.Empty;
    private Guid? _accountParentId;

    [RelayCommand]
    private async Task InitializeAsync(CancellationToken ct) => await LoadParentsAsync(ct).ConfigureAwait(true);

    [RelayCommand]
    private async Task LoadParentsAsync(CancellationToken ct)
    {
        IsLoading = true; ErrorMessage = null;
        try
        {
            var response = await _parentApi.GetParentsPagedAsync(1, 100, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
                ActiveParents = new ObservableCollection<ParentResponse>(response.Content.Items);
        }
        catch (Exception ex) when (ex is not OperationCanceledException) { ErrorMessage = $"Помилка: {ex.Message}"; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void OpenCreateForm()
    {
        IsEditing = false; FormTitle = "Додати профіль батька/матері"; FormParentId = null;
        FormLastName = string.Empty; FormFirstName = string.Empty; FormMiddleName = string.Empty; FormPhone = string.Empty;
        _formRowVersion = null; ErrorMessage = null; IsFormOpen = true;
    }

    [RelayCommand]
    private async Task OpenEditFormAsync(ParentResponse parent, CancellationToken ct)
    {
        IsLoading = true; ErrorMessage = null;
        try
        {
            var response = await _parentApi.GetParentByIdAsync(parent.ParentId, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                var p = response.Content;
                IsEditing = true; FormTitle = "Редагування профілю"; FormParentId = p.ParentId;
                FormLastName = p.LastName; FormFirstName = p.FirstName; FormMiddleName = p.MiddleName;
                FormPhone = p.Phone; _formRowVersion = p.RowVersionBase64; IsFormOpen = true;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException) { ErrorMessage = $"Помилка: {ex.Message}"; }
        finally { IsLoading = false; }
    }

    [RelayCommand] private void CloseForm() => IsFormOpen = false;

    [RelayCommand]
    private async Task SaveFormAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(FormLastName) || string.IsNullOrWhiteSpace(FormFirstName))
        {
            ErrorMessage = "Прізвище та ім'я є обов'язковими."; return;
        }

        IsLoading = true; ErrorMessage = null;
        try
        {
            IApiResponse response;
            if (IsEditing && FormParentId.HasValue && _formRowVersion is not null)
            {
                response = await _parentApi.UpdateParentAsync(FormParentId.Value, new UpdateParentRequest(FormLastName!, FormFirstName!, FormMiddleName, FormPhone, _formRowVersion), ct).ConfigureAwait(true);
            }
            else
            {
                response = await _parentApi.CreateParentAsync(new CreateParentRequest(FormLastName!, FormFirstName!, FormMiddleName, FormPhone), ct).ConfigureAwait(true);
            }

            if (response.IsSuccessStatusCode) { IsFormOpen = false; await LoadParentsAsync(ct).ConfigureAwait(true); }
            else ErrorMessage = "Сталася помилка при збереженні.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException) { ErrorMessage = $"Помилка: {ex.Message}"; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task ToggleStatusAsync(ParentResponse parent, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var response = await _parentApi.ToggleParentStatusAsync(parent.ParentId, new ToggleParentStatusRequest(parent.RowVersionBase64), ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode) await LoadParentsAsync(ct).ConfigureAwait(true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException) { ErrorMessage = $"Помилка: {ex.Message}"; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task DeleteParentAsync(ParentResponse parent, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var response = await _parentApi.DeleteParentAsync(parent.ParentId, new DeleteParentRequest(parent.RowVersionBase64), ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode) ActiveParents.Remove(parent);
        }
        catch (Exception ex) when (ex is not OperationCanceledException) { ErrorMessage = $"Помилка: {ex.Message}"; }
        finally { IsLoading = false; }
    }

    // --- ОБЛІКОВИЙ ЗАПИС ---
    [RelayCommand]
    private void OpenAccountForm(ParentResponse parent)
    {
        _accountParentId = parent.ParentId; AccountLogin = string.Empty; AccountPassword = string.Empty; ErrorMessage = null; IsAccountFormOpen = true;
    }

    [RelayCommand] private void CloseAccountForm() => IsAccountFormOpen = false;

    [RelayCommand]
    private async Task SaveAccountAsync(CancellationToken ct)
    {
        if (_accountParentId is null) return;
        if (string.IsNullOrWhiteSpace(AccountLogin) || string.IsNullOrWhiteSpace(AccountPassword))
        {
            ErrorMessage = "Логін та пароль є обов'язковими для реєстрації!"; return;
        }

        IsLoading = true; ErrorMessage = null;
        try
        {
            var detail = await _parentApi.GetParentByIdAsync(_accountParentId.Value, ct).ConfigureAwait(true);
            if (!detail.IsSuccessStatusCode || detail.Content is null) { ErrorMessage = "Не вдалося отримати деталі профілю з бази."; return; }

            var newUserId = await RegisterParentAccountAsync(AccountLogin, AccountPassword, ct).ConfigureAwait(true);
            if (newUserId is null) return;

            var linkResponse = await _parentApi.LinkParentToUserAsync(_accountParentId.Value, new LinkParentToUserRequest(newUserId.Value, detail.Content.RowVersionBase64), ct).ConfigureAwait(true);
            if (linkResponse.IsSuccessStatusCode)
            {
                IsAccountFormOpen = false;
                ErrorMessage = null;
                await LoadParentsAsync(ct).ConfigureAwait(true);
            }
            else
            {
                // Витягуємо детальне повідомлення про помилку від бекенду
                string errorDetail = linkResponse.StatusCode?.ToString() ?? "Unknown Error";
                if (linkResponse.Error is ApiException error)
                {
                    try
                    {
                        var problem = await error.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true);
                        errorDetail = problem?["title"]?.ToString() ?? errorDetail;
                    }
                    catch (System.Text.Json.JsonException)
                    {
                        // Ігноруємо помилки парсингу
                    }
                }
                ErrorMessage = $"Помилка валідації: {errorDetail}";
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException) { ErrorMessage = $"Помилка: {ex.Message}"; }
        finally { IsLoading = false; }
    }

    private async Task<Guid?> RegisterParentAccountAsync(string login, string password, CancellationToken ct)
    {
        var registerResponse = await _identityApi.RegisterAsync(new RegisterRequest(login, password, RoleType.Parent), ct).ConfigureAwait(true);
        if (registerResponse.IsSuccessStatusCode && registerResponse.Content is not null) return registerResponse.Content.UserId;

        string errorDetail = registerResponse.StatusCode?.ToString() ?? "Unknown Error";
        if (registerResponse.Error is ApiException error)
        {
            try { var problem = await error.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true); errorDetail = problem?["title"]?.ToString() ?? errorDetail; }
            catch (System.Text.Json.JsonException)
            {
                // Безпечно ігноруємо помилки парсингу, якщо сервер повернув пошкоджений JSON або HTML
            }
        }
        ErrorMessage = $"Помилка створення акаунта: {errorDetail}"; return null;
    }
}