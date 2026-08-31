using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Contracts.DTOs.Reference.LessonTypes;
using SchoolJournal.Contracts.Enums.Identity;
using System.Collections.ObjectModel;

namespace SchoolJournal.Client.Core.Features.Reference.LessonType;

public sealed partial class LessonTypesViewModel : ObservableObject
{
    private readonly ILessonTypeApi _lessonTypeApi;
    private readonly IIdentityService _identityService;

    public LessonTypesViewModel(ILessonTypeApi lessonTypeApi, IIdentityService identityService)
    {
        _lessonTypeApi = lessonTypeApi;
        _identityService = identityService;

        IsAdminOrDirector = _identityService.IsInRole(RoleType.Admin, RoleType.Director);

        _ = InitializeCommand.ExecuteAsync(null);
    }

    [ObservableProperty] private ObservableCollection<LessonTypeResponse> _activeLessonTypes = [];
    [ObservableProperty] private ObservableCollection<LessonTypeResponse> _archivedLessonTypes = [];
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isAdminOrDirector;
    [ObservableProperty] private bool _isFormOpen;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private string _formTitle = string.Empty;
    [ObservableProperty] private Guid? _formLessonTypeId;
    [ObservableProperty] private string _formTypeName = string.Empty;

    [RelayCommand]
    private async Task InitializeAsync(CancellationToken ct)
    {
        await Task.WhenAll(LoadActiveLessonTypesAsync(ct), LoadArchiveAsync(ct)).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task LoadActiveLessonTypesAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var response = await _lessonTypeApi.GetActiveLessonTypesAsync(1, 100, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
                ActiveLessonTypes = new ObservableCollection<LessonTypeResponse>(response.Content.Items);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task LoadArchiveAsync(CancellationToken ct)
    {
        if (!IsAdminOrDirector) return;
        IsLoading = true;
        try
        {
            var response = await _lessonTypeApi.GetDeletedLessonTypesAsync(1, 50, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
                ArchivedLessonTypes = new ObservableCollection<LessonTypeResponse>(response.Content.Items);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = "Не вдалося завантажити архів типів уроків.";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void OpenCreateForm()
    {
        IsEditing = false;
        FormTitle = "Створення типу уроку";
        FormLessonTypeId = null;
        FormTypeName = string.Empty;
        ErrorMessage = null;
        IsFormOpen = true;
    }

    [RelayCommand]
    private void OpenEditForm(LessonTypeResponse lessonType)
    {
        IsEditing = true;
        FormTitle = "Редагування типу уроку";
        FormLessonTypeId = lessonType.LessonTypeId;
        FormTypeName = lessonType.TypeName;
        ErrorMessage = null;
        IsFormOpen = true;
    }

    [RelayCommand] private void CloseForm() => IsFormOpen = false;

    [RelayCommand]
    private async Task SaveFormAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(FormTypeName))
        {
            ErrorMessage = "Назва не може бути порожньою.";
            return;
        }

        IsLoading = true;
        try
        {
            IApiResponse response = IsEditing && FormLessonTypeId.HasValue
                ? await _lessonTypeApi.UpdateLessonTypeAsync(FormLessonTypeId.Value, new UpdateLessonTypeRequest(FormTypeName), ct).ConfigureAwait(true)
                : await _lessonTypeApi.CreateLessonTypeAsync(new CreateLessonTypeRequest(FormTypeName), ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                IsFormOpen = false;
                await InitializeAsync(ct).ConfigureAwait(true);
            }
            else { ErrorMessage = "Помилка збереження. Перевірте унікальність назви."; }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not StackOverflowException)
        {
            ErrorMessage = $"Системна помилка: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task DeleteLessonTypeAsync(LessonTypeResponse item, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var response = await _lessonTypeApi.DeleteLessonTypeAsync(item.LessonTypeId, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode) await InitializeAsync(ct).ConfigureAwait(true);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task RestoreLessonTypeAsync(LessonTypeResponse item, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var response = await _lessonTypeApi.RestoreLessonTypeAsync(item.LessonTypeId, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode) await InitializeAsync(ct).ConfigureAwait(true);
        }
        finally { IsLoading = false; }
    }
}