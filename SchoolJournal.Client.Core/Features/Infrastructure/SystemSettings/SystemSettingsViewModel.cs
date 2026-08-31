using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Contracts.DTOs.Infrastructure.SystemSettings;
using SchoolJournal.Contracts.Enums.Identity;
using System.Net;

namespace SchoolJournal.Client.Core.Features.Infrastructure.SystemSettings;

public sealed partial class SystemSettingsViewModel : ObservableObject
{
    private readonly ISystemSettingsApi _api;
    private readonly IIdentityService _identityService;
    private string? _rowVersion;

    public SystemSettingsViewModel(ISystemSettingsApi api, IIdentityService identityService)
    {
        _api = api;
        _identityService = identityService;

        CanEdit = _identityService.IsInRole(RoleType.Admin, RoleType.Director);

        _ = LoadSettingsCommand.ExecuteAsync(null);
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    [ObservableProperty]
    private bool _canEdit;

    [ObservableProperty]
    private string _schoolName = string.Empty;

    [ObservableProperty]
    private string _academicYear = string.Empty;

    [ObservableProperty]
    private string? _principalName;

    [RelayCommand]
    private async Task LoadSettingsAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            var response = await _api.GetSettingsAsync(ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                SchoolName = response.Content.SchoolName;
                AcademicYear = response.Content.AcademicYear;
                PrincipalName = response.Content.PrincipalName;
                _rowVersion = response.Content.RowVersionBase64;
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                ErrorMessage = CanEdit
                    ? "Налаштування ще не створені. Заповніть форму та збережіть."
                    : "Системні налаштування ще не задані адміністратором.";
            }
            else
            {
                ErrorMessage = $"Помилка завантаження: {response.Error?.Message}";
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Системна помилка: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveSettingsAsync(CancellationToken ct)
    {
        if (!CanEdit) return;

        if (string.IsNullOrWhiteSpace(SchoolName) || string.IsNullOrWhiteSpace(AcademicYear))
        {
            ErrorMessage = "Назва школи та Академічний рік є обов'язковими.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            var request = new UpdateSystemSettingsRequest(SchoolName, AcademicYear, PrincipalName, _rowVersion);
            var response = await _api.UpdateSettingsAsync(request, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                SuccessMessage = "Налаштування успішно збережено!";
                await LoadSettingsAsync(ct).ConfigureAwait(true); 
            }
            else if (response.Error is not null)
            {
var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                var serverMessage = problem?["title"]?.ToString();

                ErrorMessage = response.StatusCode switch
                {
                    HttpStatusCode.Conflict => "Конфлікт даних: налаштування змінені іншим користувачем. Оновіть сторінку.",
                    HttpStatusCode.BadRequest => serverMessage ?? "Некоректні дані.",
                    _ => $"Помилка API: {response.Error.Message}"
                };
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Системна помилка: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}