using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Client.Core.Features.Core.Subgroups;
using SchoolJournal.Contracts.DTOs.Core.StudentSubgroups;
using SchoolJournal.Contracts.DTOs.Core.Subgroups;
using SchoolJournal.Contracts.Enums.Identity;
using SchoolJournal.Client.Core.Features.Core.SchoolClasses;
using SchoolJournal.Contracts.DTOs.Core.SchoolClasses;

namespace SchoolJournal.Client.Core.Features.Core.StudentSubgroups;

public sealed partial class StudentSubgroupsViewModel : ObservableObject
{
    private readonly IStudentSubgroupsApi _api;
    private readonly ISubgroupsApi _subgroupsApi;
    private readonly ISchoolClassApi _schoolClassApi;
    private readonly IIdentityService _identityService;

    public StudentSubgroupsViewModel(
        IStudentSubgroupsApi api,
        ISubgroupsApi subgroupsApi,
        ISchoolClassApi schoolClassApi,
        IIdentityService identityService)
    {
        _api = api;
        _subgroupsApi = subgroupsApi;
        _schoolClassApi = schoolClassApi;
        _identityService = identityService;
        IsAdminOrDirector = _identityService.IsInRole(RoleType.Admin, RoleType.Director);

        _ = LoadClassesAsync(default);
    }

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isAdminOrDirector;
    [ObservableProperty] private SubgroupStudentsDetail? _currentSubgroupDetails;
    [ObservableProperty] private ObservableCollection<SubgroupStudentModel> _students = [];
    [ObservableProperty] private ObservableCollection<SchoolClassItemResponse> _availableClasses = [];
    [ObservableProperty] private ObservableCollection<SubgroupResponse> _availableSubgroups = [];
    [ObservableProperty] private ObservableCollection<AvailableStudentModel> _availableStudents = [];

    [ObservableProperty]
    private AvailableStudentModel? _selectedAvailableStudent;

    [ObservableProperty]
    private SubgroupResponse? _selectedSubgroup;

    [ObservableProperty]
    private SchoolClassItemResponse? _selectedClass;

    partial void OnSelectedClassChanged(SchoolClassItemResponse? value)
    {
        SelectedSubgroup = null;
        AvailableSubgroups.Clear();
        Students.Clear();
        AvailableStudents.Clear();

        if (value is not null)
        {
            _ = LoadSubgroupsByClassAsync(value.ClassId, default);
        }
    }

    partial void OnSelectedSubgroupChanged(SubgroupResponse? value)
    {
        if (value is not null)
        {
            LoadStudentsCommand.Execute(null);
            LoadAvailableStudentsCommand.Execute(null);
        }
        else
        {
            Students.Clear();
            AvailableStudents.Clear();
            CurrentSubgroupDetails = null;
            SelectedAvailableStudent = null;
        }
    }

    private async Task LoadClassesAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var response = await _schoolClassApi.GetActiveClassesAsync(1, 1000, null, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                AvailableClasses = new ObservableCollection<SchoolClassItemResponse>(response.Content.Items);
            }
            else
            {
                ErrorMessage = "Не вдалося завантажити список класів.";
            }
        }
        catch (Refit.ApiException ex)
        {
            ErrorMessage = $"Помилка сервера при завантаженні класів: {ex.StatusCode}";
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

    private async Task LoadSubgroupsByClassAsync(Guid classId, CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var response = await _subgroupsApi.GetSubgroupsByClassAsync(classId, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                AvailableSubgroups = new ObservableCollection<SubgroupResponse>(response.Content);
            }
            else
            {
                ErrorMessage = "Не вдалося завантажити список підгруп для цього класу.";
            }
        }
        catch (Refit.ApiException ex)
        {
            ErrorMessage = $"Помилка сервера при завантаженні підгруп: {ex.StatusCode}";
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

    [RelayCommand]
    public async Task LoadStudentsAsync(CancellationToken ct)
    {
        if (SelectedSubgroup is null) return;

        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var response = await _api.GetStudentsBySubgroupAsync(SelectedSubgroup.SubgroupId, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                CurrentSubgroupDetails = response.Content;
                Students = new ObservableCollection<SubgroupStudentModel>(response.Content.Students);
            }
            else
            {
                ErrorMessage = "Не вдалося завантажити список студентів підгрупи.";
                Students.Clear();
            }
        }
        catch (Refit.ApiException ex)
        {
            ErrorMessage = $"Помилка сервера: {ex.StatusCode}";
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

    [RelayCommand]
    public async Task LoadAvailableStudentsAsync(CancellationToken ct)
    {
        if (SelectedSubgroup is null) return;

        IsLoading = true;
        try
        {
            var response = await _api.GetAvailableStudentsAsync(SelectedSubgroup.SubgroupId, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                AvailableStudents = new ObservableCollection<AvailableStudentModel>(response.Content);
            }
            else
            {
                AvailableStudents.Clear();
            }
        }
        catch (Refit.ApiException ex)
        {
            ErrorMessage = $"Помилка сервера при завантаженні доступних студентів: {ex.StatusCode}";
            AvailableStudents.Clear();
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Помилка мережі: {ex.Message}";
            AvailableStudents.Clear();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AssignStudentAsync(CancellationToken ct)
    {
        if (SelectedSubgroup is null || SelectedAvailableStudent is null) return;

        IsLoading = true;
        try
        {
            var request = new AssignStudentToSubgroupRequest(SelectedAvailableStudent.StudentId, SelectedSubgroup.SubgroupId);
            var response = await _api.AssignStudentToSubgroupAsync(request, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                await LoadStudentsAsync(ct).ConfigureAwait(true);
                await LoadAvailableStudentsAsync(ct).ConfigureAwait(true);
                SelectedAvailableStudent = null;
            }
            else
            {
                ErrorMessage = "Помилка при додаванні студента до підгрупи.";
            }
        }
        catch (Refit.ApiException ex)
        {
            ErrorMessage = $"Помилка сервера: {ex.StatusCode}";
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

    [RelayCommand]
    private async Task RemoveStudentAsync(SubgroupStudentModel student, CancellationToken ct)
    {
        if (student is null) return;

        IsLoading = true;
        try
        {
            var response = await _api.RemoveStudentFromSubgroupAsync(student.StudentSubgroupId, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode)
            {
                Students.Remove(student);
                await LoadAvailableStudentsAsync(ct).ConfigureAwait(true);
            }
            else
            {
                ErrorMessage = "Помилка при видаленні студента з підгрупи.";
            }

        }
        catch (Refit.ApiException ex)
        {
            ErrorMessage = $"Помилка сервера: {ex.StatusCode}";
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
}