using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Client.Core.Features.Core.Parents;
using SchoolJournal.Client.Core.Features.Core.Students;
using SchoolJournal.Contracts.DTOs.Core.Parents;
using SchoolJournal.Contracts.DTOs.Core.StudentParents;
using SchoolJournal.Contracts.DTOs.Core.Students;
using SchoolJournal.Contracts.Enums.Identity;
using System.Collections.ObjectModel;

namespace SchoolJournal.Client.Core.Features.Core.StudentParents;

public sealed partial class StudentParentsViewModel : ObservableObject
{
    private readonly IStudentParentApi _studentParentApi;
    private readonly IIdentityService _identityService;
    private readonly IParentApi _parentApi;     // ДОДАНО: Щоб дістати список для випадаючого вікна
    private readonly IStudentApi _studentApi;   // ДОДАНО: Щоб дістати список учнів

    public StudentParentsViewModel(IStudentParentApi studentParentApi, IIdentityService identityService, IParentApi parentApi, IStudentApi studentApi)
    {
        _studentParentApi = studentParentApi;
        _identityService = identityService;
        _parentApi = parentApi;
        _studentApi = studentApi;
        IsAdminOrDirector = _identityService.IsInRole(RoleType.Admin, RoleType.Director);
    }

    [ObservableProperty] public partial ObservableCollection<StudentParentDetailResponse> ParentsForStudent { get; set; } = [];
    [ObservableProperty] public partial ObservableCollection<ParentStudentDetailResponse> StudentsForParent { get; set; } = [];

    // Списки для вибору у випадаючому меню
    [ObservableProperty] public partial ObservableCollection<ParentResponse> AvailableParents { get; set; } = [];
    [ObservableProperty] public partial ObservableCollection<StudentSearchResponse> AvailableStudents { get; set; } = [];

    [ObservableProperty] public partial bool IsLoading { get; set; }
    [ObservableProperty] public partial string? ErrorMessage { get; set; }
    [ObservableProperty] public partial bool IsAdminOrDirector { get; set; }
    [ObservableProperty] public partial bool IsStudentContext { get; set; }

    private Guid? _currentContextId;

    // --- ФОРМА ДОДАВАННЯ ЗВ'ЯЗКУ ---
    [ObservableProperty] public partial bool IsAddFormOpen { get; set; }
    [ObservableProperty] public partial Guid? SelectedAssignId { get; set; } // ParentId або StudentId
    [ObservableProperty] public partial string AssignRole { get; set; } = "Мати";

    [RelayCommand]
    public async Task LoadForStudentAsync(Guid studentId, CancellationToken ct)
    {
        IsStudentContext = true; _currentContextId = studentId; IsLoading = true; ErrorMessage = null;
        try
        {
            var response = await _studentParentApi.GetParentsByStudentIdAsync(studentId, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
                ParentsForStudent = new ObservableCollection<StudentParentDetailResponse>(response.Content);
        }
        catch (Exception ex) when (ex is not OperationCanceledException) { ErrorMessage = $"Системна помилка: {ex.Message}"; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task LoadForParentAsync(Guid parentId, CancellationToken ct)
    {
        IsStudentContext = false; _currentContextId = parentId; IsLoading = true; ErrorMessage = null;
        try
        {
            var response = await _studentParentApi.GetStudentsByParentIdAsync(parentId, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
                StudentsForParent = new ObservableCollection<ParentStudentDetailResponse>(response.Content);
        }
        catch (Exception ex) when (ex is not OperationCanceledException) { ErrorMessage = $"Системна помилка: {ex.Message}"; }
        finally { IsLoading = false; }
    }

    // --- ЛОГІКА ДОДАВАННЯ ---
    [RelayCommand]
    private async Task OpenAddFormAsync(CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            // Завантажуємо списки для ComboBox залежно від контексту
            if (IsStudentContext)
            {
                var pResp = await _parentApi.GetParentsPagedAsync(1, 1000, ct).ConfigureAwait(true);
                if (pResp.IsSuccessStatusCode && pResp.Content is not null)
                    AvailableParents = new ObservableCollection<ParentResponse>(pResp.Content.Items);
            }
            else
            {
                var sResp = await _studentApi.SearchStudentsAsync(null, null, true, 1, 1000, ct).ConfigureAwait(true);
                if (sResp.IsSuccessStatusCode && sResp.Content is not null)
                    AvailableStudents = new ObservableCollection<StudentSearchResponse>(sResp.Content.Items);
            }

            SelectedAssignId = null;
            AssignRole = "Мати";
            ErrorMessage = null;
            IsAddFormOpen = true;
        }
        finally { IsLoading = false; }
    }

    [RelayCommand] private void CloseAddForm() => IsAddFormOpen = false;

    [RelayCommand]
    private async Task SaveAssignAsync(CancellationToken ct)
    {
        if (_currentContextId is null || SelectedAssignId is null || string.IsNullOrWhiteSpace(AssignRole))
        {
            ErrorMessage = "Оберіть профіль та вкажіть роль."; return;
        }

        IsLoading = true;
        try
        {
            Guid stId = IsStudentContext ? _currentContextId.Value : SelectedAssignId.Value;
            Guid pId = IsStudentContext ? SelectedAssignId.Value : _currentContextId.Value;

            var request = new AssignParentToStudentRequest(pId, stId, AssignRole);
            var response = await _studentParentApi.AssignParentToStudentAsync(request, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                IsAddFormOpen = false;
                await ReloadCurrentContextAsync(ct).ConfigureAwait(true);
            }
            else ErrorMessage = "Помилка додавання зв'язку. Можливо, він вже існує.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException) { ErrorMessage = $"Помилка: {ex.Message}"; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task RemoveRelationAsync(Guid studentParentId, CancellationToken ct)
    {
        if (!IsAdminOrDirector) return;
        IsLoading = true;
        try
        {
            var response = await _studentParentApi.RemoveParentFromStudentAsync(studentParentId, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode) await ReloadCurrentContextAsync(ct).ConfigureAwait(true);
            else ErrorMessage = "Помилка видалення зв'язку.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException) { ErrorMessage = $"Помилка: {ex.Message}"; }
        finally { IsLoading = false; }
    }

    private async Task ReloadCurrentContextAsync(CancellationToken ct)
    {
        if (_currentContextId.HasValue)
        {
            if (IsStudentContext) await LoadForStudentAsync(_currentContextId.Value, ct).ConfigureAwait(true);
            else await LoadForParentAsync(_currentContextId.Value, ct).ConfigureAwait(true);
        }
    }
}