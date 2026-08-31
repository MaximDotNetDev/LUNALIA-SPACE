using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Client.Core.Features.Core.SchoolClasses;
using SchoolJournal.Client.Core.Features.Core.Subgroups;
using SchoolJournal.Client.Core.Features.Core.Subject;
using SchoolJournal.Client.Core.Features.Core.Teachers;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Core.SchoolClasses;
using SchoolJournal.Contracts.DTOs.Core.Subgroups;
using SchoolJournal.Contracts.DTOs.Core.Subjects;
using SchoolJournal.Contracts.DTOs.Core.Teachers;
using SchoolJournal.Contracts.DTOs.Operations.TeachingAssignments;
using SchoolJournal.Contracts.Enums.Identity;
using System.Collections.ObjectModel;

namespace SchoolJournal.Client.Core.Features.Operations.TeachingAssignments;

public sealed partial class TeachingAssignmentsViewModel : ObservableObject
{
    private readonly ITeachingAssignmentApi _api;
    private readonly IIdentityService _identityService;
    private readonly ITeacherApi _teacherApi;
    private readonly ISubjectApi _subjectApi;
    private readonly ISchoolClassApi _classApi;
    private readonly ISubgroupsApi _subgroupsApi;

    public TeachingAssignmentsViewModel(
        ITeachingAssignmentApi api,
        IIdentityService identityService,
        ITeacherApi teacherApi,
        ISubjectApi subjectApi,
        ISchoolClassApi classApi,
        ISubgroupsApi subgroupsApi)
    {
        _api = api;
        _identityService = identityService;
        _teacherApi = teacherApi;
        _subjectApi = subjectApi;
        _classApi = classApi;
        _subgroupsApi = subgroupsApi;
        IsAdminOrDirector = _identityService.IsInRole(RoleType.Admin, RoleType.Director);

        _ = InitializeFilterAsync();
    }

    [ObservableProperty] private ObservableCollection<TeachingAssignmentResponse> _assignments = [];
    [ObservableProperty] private ObservableCollection<TeacherListItemResponse> _teachers = [];
    [ObservableProperty] private ObservableCollection<SubjectResponse> _subjects = [];
    [ObservableProperty] private ObservableCollection<SchoolClassItemResponse> _classes = [];
    [ObservableProperty] private ObservableCollection<SubgroupResponse> _subgroups = [];
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isAdminOrDirector;
    [ObservableProperty] private bool _isFormOpen;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private string _formTitle = string.Empty;

    // Filter Field
    private SchoolClassItemResponse? _selectedFilterClass;
    public SchoolClassItemResponse? SelectedFilterClass
    {
        get => _selectedFilterClass;
        set
        {
            if (SetProperty(ref _selectedFilterClass, value))
            {
                if (value is not null)
                {
                    _ = LoadByClassCommand.ExecuteAsync(value.ClassId);
                }
                else
                {
                    Assignments.Clear();
                }
            }
        }
    }

    private async Task InitializeFilterAsync()
    {
        IsLoading = true;
        try
        {
            var response = await _classApi.GetActiveClassesAsync(1, 1000, null).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                Classes = new ObservableCollection<SchoolClassItemResponse>(response.Content.Items);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження класів: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Form Fields
    [ObservableProperty] private Guid? _formAssignmentId;
    [ObservableProperty] private Guid _formTeacherId;
    [ObservableProperty] private Guid _formSubjectId;
    [ObservableProperty] private Guid _formClassId;
    [ObservableProperty] private Guid? _formSubgroupId;
    private string? _formRowVersion;

    [RelayCommand]
    private async Task LoadByClassAsync(Guid classId, CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var response = await _api.GetByClassIdAsync(classId, 1, 100, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                Assignments = new ObservableCollection<TeachingAssignmentResponse>(response.Content.Items);
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

    private async Task LoadReferenceDataAsync(CancellationToken ct)
    {
        try
        {
            var teachersTask = _teacherApi.GetTeachersAsync(1, 1000, null, null, true, ct);
            var subjectsTask = _subjectApi.GetActiveSubjectsAsync(1, 1000, null, ct);
            var classesTask = _classApi.GetActiveClassesAsync(1, 1000, null, ct);
            var subgroupsTask = _subgroupsApi.GetSubgroupsListAsync(1, 1000, ct);

            await Task.WhenAll(teachersTask, subjectsTask, classesTask, subgroupsTask).ConfigureAwait(true);

            AssignTeachers(await teachersTask.ConfigureAwait(true));
            AssignSubjects(await subjectsTask.ConfigureAwait(true));
            AssignClasses(await classesTask.ConfigureAwait(true));
            AssignSubgroups(await subgroupsTask.ConfigureAwait(true));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження довідників: {ex.Message}";
        }
    }

    private void AssignTeachers(IApiResponse<PagedResponse<TeacherListItemResponse>> res)
    {
        if (res.IsSuccessStatusCode && res.Content is not null)
            Teachers = new ObservableCollection<TeacherListItemResponse>(res.Content.Items);
    }

    private void AssignSubjects(IApiResponse<PagedResponse<SubjectResponse>> res)
    {
        if (res.IsSuccessStatusCode && res.Content is not null)
            Subjects = new ObservableCollection<SubjectResponse>(res.Content.Items);
    }

    private void AssignClasses(IApiResponse<PagedResponse<SchoolClassItemResponse>> res)
    {
        if (res.IsSuccessStatusCode && res.Content is not null)
            Classes = new ObservableCollection<SchoolClassItemResponse>(res.Content.Items);
    }

    private void AssignSubgroups(IApiResponse<PagedResponse<SubgroupResponse>> res)
    {
        if (res.IsSuccessStatusCode && res.Content is not null)
            Subgroups = new ObservableCollection<SubgroupResponse>(res.Content.Items);
    }

    [RelayCommand]
    private async Task OpenCreateFormAsync(CancellationToken ct)
    {
        IsLoading = true;
        await LoadReferenceDataAsync(ct).ConfigureAwait(true);
        IsLoading = false;

        IsEditing = false;
        FormTitle = "Нове призначення";
        FormAssignmentId = null;
        FormTeacherId = Guid.Empty;
        FormSubjectId = Guid.Empty;
        FormClassId = Guid.Empty;
        FormSubgroupId = null;
        _formRowVersion = null;
        ErrorMessage = null;
        IsFormOpen = true;
    }

    [RelayCommand]
    private async Task OpenEditFormAsync(TeachingAssignmentResponse assignment, CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            await LoadReferenceDataAsync(ct).ConfigureAwait(true);
            await PopulateEditFormAsync(assignment.AssignmentId, ct).ConfigureAwait(true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження даних: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task PopulateEditFormAsync(Guid assignmentId, CancellationToken ct)
    {
        var response = await _api.GetByIdAsync(assignmentId, ct).ConfigureAwait(true);
        if (response.IsSuccessStatusCode && response.Content is not null)
        {
            var fresh = response.Content;
            IsEditing = true;
            FormTitle = "Редагування призначення";
            FormAssignmentId = fresh.AssignmentId;
            FormTeacherId = fresh.TeacherId;
            FormSubjectId = fresh.SubjectId;
            FormClassId = fresh.ClassId;
            FormSubgroupId = fresh.SubgroupId;
            _formRowVersion = fresh.RowVersionBase64;
            IsFormOpen = true;
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
        if (FormTeacherId == Guid.Empty || FormSubjectId == Guid.Empty || FormClassId == Guid.Empty)
        {
            ErrorMessage = "Вчитель, Предмет та Клас є обов'язковими.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;
        try
        {
            await ProcessSaveAsync(ct).ConfigureAwait(true);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not StackOverflowException and not OperationCanceledException)
        {
            ErrorMessage = $"Системна помилка: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ProcessSaveAsync(CancellationToken ct)
    {
        IApiResponse response;
        if (IsEditing && FormAssignmentId.HasValue && _formRowVersion is not null)
        {
            var req = new UpdateTeachingAssignmentRequest(FormTeacherId, FormSubjectId, FormClassId, FormSubgroupId, _formRowVersion);
            response = await _api.UpdateAsync(FormAssignmentId.Value, req, ct).ConfigureAwait(true);
        }
        else
        {
            var req = new CreateTeachingAssignmentRequest(FormTeacherId, FormSubjectId, FormClassId, FormSubgroupId);
            response = await _api.CreateAsync(req, ct).ConfigureAwait(true);
        }

        if (response.IsSuccessStatusCode)
        {
            IsFormOpen = false;
        }
        else if (response.Error is not null)
        {
            ErrorMessage = $"Помилка збереження: {response.StatusCode}";
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(TeachingAssignmentResponse assignment, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var req = new DeleteTeachingAssignmentRequest(assignment.RowVersionBase64);
            var response = await _api.DeleteAsync(assignment.AssignmentId, req, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode)
            {
                Assignments.Remove(assignment);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка видалення: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ToggleStatusAsync(TeachingAssignmentResponse assignment, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var req = new ToggleTeachingAssignmentStatusRequest(assignment.RowVersionBase64);
            await _api.ToggleStatusAsync(assignment.AssignmentId, req, ct).ConfigureAwait(true);

        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка зміни статусу: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}