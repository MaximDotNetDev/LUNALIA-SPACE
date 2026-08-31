using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Client.Core.Features.Core.Teachers;
using SchoolJournal.Contracts.DTOs.Core.SchoolClasses;
using SchoolJournal.Contracts.DTOs.Core.Teachers;
using SchoolJournal.Contracts.Enums.Identity;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SchoolJournal.Client.Core.Features.Core.SchoolClasses;

public sealed partial class SchoolClassesViewModel : ObservableObject
{
    private readonly ISchoolClassApi _classApi;
    private readonly ITeacherApi _teacherApi;
    private readonly IIdentityService _identityService;

    public SchoolClassesViewModel(ISchoolClassApi classApi, ITeacherApi teacherApi, IIdentityService identityService)
    {
        _classApi = classApi;
        _teacherApi = teacherApi;
        _identityService = identityService;

        IsAdminOrDirector = _identityService.IsInRole(RoleType.Admin, RoleType.Director);
        _ = LoadInitialDataCommand.ExecuteAsync(null);
    }

    [ObservableProperty] public partial ObservableCollection<SchoolClassItemResponse> Classes { get; set; } = [];
    [ObservableProperty] public partial ObservableCollection<TeacherListItemResponse> Teachers { get; set; } = [];
    [ObservableProperty] public partial bool IsLoading { get; set; }
    [ObservableProperty] public partial string? ErrorMessage { get; set; }
    [ObservableProperty] public partial bool IsAdminOrDirector { get; set; }

    // Form Properties
    [ObservableProperty] public partial bool IsFormOpen { get; set; }
    [ObservableProperty] public partial string FormTitle { get; set; } = string.Empty;
    [ObservableProperty] public partial Guid? EditingClassId { get; set; }
    [ObservableProperty] public partial string ClassName { get; set; } = string.Empty;
    [ObservableProperty] public partial int GradeLevel { get; set; } = 1;
    [ObservableProperty] public partial string AcademicYear { get; set; } = $"{DateTime.Today.Year}-{DateTime.Today.Year + 1}";
    [ObservableProperty] public partial TeacherListItemResponse? SelectedTeacher { get; set; }

    private string? _currentRowVersion;

    [RelayCommand]
    private async Task LoadInitialDataAsync(CancellationToken ct)
    {
        await Task.WhenAll(LoadClassesAsync(ct), LoadTeachersAsync(ct)).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task LoadClassesAsync(CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var response = await _classApi.GetActiveClassesAsync(1, 100, null, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
                Classes = new ObservableCollection<SchoolClassItemResponse>(response.Content.Items);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = "Помилка завантаження класів.";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task LoadTeachersAsync(CancellationToken ct)
    {
        try
        {
            var response = await _teacherApi.GetTeachersAsync(1, 500, isActive: true, ct: ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                Teachers = new ObservableCollection<TeacherListItemResponse>(response.Content.Items);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження вчителів: {ex.Message}";
        }
    }

    [RelayCommand]
    private void OpenCreateForm()
    {
        FormTitle = "Створення класу";
        EditingClassId = null;
        ClassName = string.Empty;
        GradeLevel = 1;
        SelectedTeacher = null;
        _currentRowVersion = null;
        ErrorMessage = null;
        IsFormOpen = true;
    }

    [RelayCommand]
    private async Task OpenEditFormAsync(SchoolClassItemResponse classItem, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(classItem);

        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var response = await _classApi.GetClassByIdAsync(classItem.ClassId, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                var data = response.Content;
                FormTitle = "Редагування класу";
                EditingClassId = data.ClassId;
                ClassName = data.ClassName;
                GradeLevel = data.GradeLevel;
                AcademicYear = data.AcademicYear;
                SelectedTeacher = Teachers.FirstOrDefault(t => t.TeacherId == data.HomeroomTeacherId);
                _currentRowVersion = data.RowVersionBase64;
                IsFormOpen = true;
            }
            else
            {
                ErrorMessage = "Не вдалося отримати актуальні дані класу.";
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка при завантаженні даних: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(ClassName)) { ErrorMessage = "Назва класу обов'язкова."; return; }
        if (SelectedTeacher is null) { ErrorMessage = "Оберіть вчителя."; return; }

        IsLoading = true;
        ErrorMessage = null;
        try
        {
            IApiResponse response;
            if (EditingClassId.HasValue && _currentRowVersion is not null)
            {
                var request = new UpdateSchoolClassRequest(ClassName, GradeLevel, AcademicYear, SelectedTeacher.TeacherId, _currentRowVersion);
                response = await _classApi.UpdateClassAsync(EditingClassId.Value, request, ct).ConfigureAwait(true);
            }
            else
            {
                var request = new CreateSchoolClassRequest(ClassName, GradeLevel, AcademicYear, SelectedTeacher.TeacherId);
                response = await _classApi.CreateClassAsync(request, ct).ConfigureAwait(true);
            }

            if (response.IsSuccessStatusCode)
            {
                IsFormOpen = false;
                await LoadClassesAsync(ct).ConfigureAwait(true);
            }
            else
            {
                ErrorMessage = "Помилка при збереженні даних на сервері.";
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Системна помилка: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand] private void CloseForm() => IsFormOpen = false;
}