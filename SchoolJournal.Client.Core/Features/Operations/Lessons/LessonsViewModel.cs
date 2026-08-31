using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Client.Core.Features.Reference.BellSchedule;
using SchoolJournal.Client.Core.Features.Reference.Classroom;
using SchoolJournal.Client.Core.Features.Reference.LessonType;
using SchoolJournal.Client.Core.Features.Reference.Semester;
using SchoolJournal.Contracts.DTOs.Operations.Lessons;
using SchoolJournal.Contracts.DTOs.Reference.BellSchedules;
using SchoolJournal.Contracts.DTOs.Reference.Classrooms;
using SchoolJournal.Contracts.DTOs.Reference.LessonTypes;
using SchoolJournal.Contracts.DTOs.Reference.Semesters;
using System.Collections.ObjectModel;
using SchoolJournal.Client.Core.Features.Core.SchoolClasses;
using SchoolJournal.Client.Core.Features.Operations.TeachingAssignments;
using SchoolJournal.Contracts.DTOs.Core.SchoolClasses;
using SchoolJournal.Contracts.DTOs.Operations.TeachingAssignments;

namespace SchoolJournal.Client.Core.Features.Operations.Lessons;

public sealed partial class LessonsViewModel : ObservableObject
{
    private readonly ILessonApi lessonApi;
    private readonly ISemesterApi semesterApi;
    private readonly ILessonTypeApi lessonTypeApi;
    private readonly IBellScheduleApi bellScheduleApi;
    private readonly IClassroomApi classroomApi;
    private readonly ISchoolClassApi classApi;
    private readonly ITeachingAssignmentApi assignmentApi;

    public LessonsViewModel(
        ILessonApi lessonApi,
        ISemesterApi semesterApi,
        ILessonTypeApi lessonTypeApi,
        IBellScheduleApi bellScheduleApi,
        IClassroomApi classroomApi,
        ISchoolClassApi classApi,
        ITeachingAssignmentApi assignmentApi)
    {
        this.lessonApi = lessonApi;
        this.semesterApi = semesterApi;
        this.lessonTypeApi = lessonTypeApi;
        this.bellScheduleApi = bellScheduleApi;
        this.classroomApi = classroomApi;
        this.classApi = classApi;
        this.assignmentApi = assignmentApi;

        _ = InitializeFiltersAsync();
    }

    [ObservableProperty]
    private ObservableCollection<LessonResponse> _lessons = [];

    [ObservableProperty]
    private ObservableCollection<SemesterResponse> _semesters = [];

    [ObservableProperty]
    private ObservableCollection<LessonTypeResponse> _lessonTypes = [];

    [ObservableProperty]
    private ObservableCollection<BellScheduleResponse> _bellSchedules = [];

    [ObservableProperty]
    private ObservableCollection<ClassroomResponse> _classrooms = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private Guid? _selectedAssignmentId;

    [ObservableProperty] private ObservableCollection<SchoolClassItemResponse> _classes = [];
    [ObservableProperty] private ObservableCollection<TeachingAssignmentResponse> _assignments = [];

    private SchoolClassItemResponse? _selectedFilterClass;
    public SchoolClassItemResponse? SelectedFilterClass
    {
        get => _selectedFilterClass;
        set
        {
            if (SetProperty(ref _selectedFilterClass, value))
            {
                Assignments.Clear();
                SelectedFilterAssignment = null;
                Lessons.Clear();
                if (value is not null)
                {
                    _ = LoadAssignmentsByClassAsync(value.ClassId);
                }
            }
        }
    }

    private TeachingAssignmentResponse? _selectedFilterAssignment;
    public TeachingAssignmentResponse? SelectedFilterAssignment
    {
        get => _selectedFilterAssignment;
        set
        {
            if (SetProperty(ref _selectedFilterAssignment, value))
            {
                Lessons.Clear();
                SelectedAssignmentId = value?.AssignmentId;
                if (value is not null)
                {
                    _ = LoadLessonsAsync(value.AssignmentId, CancellationToken.None);
                }
            }
        }
    }

    private async Task InitializeFiltersAsync()
    {
        IsLoading = true;
        try
        {
            var response = await classApi.GetActiveClassesAsync(1, 1000, null).ConfigureAwait(true);
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

    private async Task LoadAssignmentsByClassAsync(Guid classId)
    {
        IsLoading = true;
        try
        {
            var response = await assignmentApi.GetByClassIdAsync(classId, 1, 1000).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                Assignments = new ObservableCollection<TeachingAssignmentResponse>(response.Content.Items);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження предметів: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [ObservableProperty]
    private bool _isFormOpen;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _formTitle = string.Empty;

    [ObservableProperty]
    private Guid? _formLessonId;

    [ObservableProperty]
    private string? _formRowVersionBase64;

    [ObservableProperty]
    private DateTime? _formLessonDate;

    [ObservableProperty]
    private SemesterResponse? _selectedSemester;

    [ObservableProperty]
    private LessonTypeResponse? _selectedLessonType;

    [ObservableProperty]
    private BellScheduleResponse? _selectedBellSchedule;

    [ObservableProperty]
    private ClassroomResponse? _selectedClassroom;

    [ObservableProperty]
    private string? _formLessonTopic;

    [ObservableProperty]
    private string? _formHomework;

    [ObservableProperty]
    private bool _isRescheduleFormOpen;

    [ObservableProperty]
    private DateTime? _rescheduleDate;

    [ObservableProperty]
    private BellScheduleResponse? _rescheduleSelectedBellSchedule;

    [ObservableProperty]
    private ClassroomResponse? _rescheduleSelectedClassroom;

    [RelayCommand]
    private async Task LoadDictionariesAsync(CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var semestersTask = semesterApi.GetActiveSemestersAsync(1, 100, ct);
            var lessonTypesTask = lessonTypeApi.GetActiveLessonTypesAsync(1, 100, ct);
            var bellSchedulesTask = bellScheduleApi.GetActiveBellSchedulesAsync(1, 100, ct);
            var classroomsTask = classroomApi.GetActiveClassroomsAsync(1, 100, null, ct);

            await Task.WhenAll(semestersTask, lessonTypesTask, bellSchedulesTask, classroomsTask).ConfigureAwait(true);

            var semesters = await semestersTask.ConfigureAwait(true);
            var lessonTypes = await lessonTypesTask.ConfigureAwait(true);
            var bellSchedules = await bellSchedulesTask.ConfigureAwait(true);
            var classrooms = await classroomsTask.ConfigureAwait(true);

            if (semesters.IsSuccessStatusCode && semesters.Content is not null)
                Semesters = new ObservableCollection<SemesterResponse>(semesters.Content.Items);

            if (lessonTypes.IsSuccessStatusCode && lessonTypes.Content is not null)
                LessonTypes = new ObservableCollection<LessonTypeResponse>(lessonTypes.Content.Items);

            if (bellSchedules.IsSuccessStatusCode && bellSchedules.Content is not null)
                BellSchedules = new ObservableCollection<BellScheduleResponse>(bellSchedules.Content.Items);

            if (classrooms.IsSuccessStatusCode && classrooms.Content is not null)
                Classrooms = new ObservableCollection<ClassroomResponse>(classrooms.Content.Items);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження довідників: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task LoadLessonsAsync(Guid assignmentId, CancellationToken ct)
    {
        SelectedAssignmentId = assignmentId;
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            if (Semesters.Count == 0)
            {
                await LoadDictionariesAsync(ct).ConfigureAwait(true);
            }

            var response = await lessonApi.GetLessonsByAssignmentAsync(assignmentId, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                Lessons = new ObservableCollection<LessonResponse>(response.Content);
            }
            else
            {
                ErrorMessage = "Не вдалося завантажити список уроків.";
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void OpenCreateForm()
    {
        if (SelectedAssignmentId is null)
        {
            ErrorMessage = "Спочатку оберіть призначення (клас/предмет).";
            return;
        }

        IsEditing = false;
        FormTitle = "Створення нового уроку";
        FormLessonId = null;
        FormRowVersionBase64 = null;
        FormLessonDate = DateTime.Today;
        FormLessonTopic = string.Empty;
        FormHomework = string.Empty;

        SelectedSemester = null;
        SelectedLessonType = null;
        SelectedBellSchedule = null;
        SelectedClassroom = null;

        ErrorMessage = null;
        IsFormOpen = true;
    }

    [RelayCommand]
    private void OpenEditForm(LessonResponse lesson)
    {
        IsEditing = true;
        FormTitle = "Оновлення теми та домашнього завдання";
        FormLessonId = lesson.LessonId;
        FormRowVersionBase64 = lesson.RowVersionBase64;

        FormLessonTopic = lesson.LessonTopic;
        FormHomework = lesson.Homework;

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
        ErrorMessage = null;

        try
        {
            IsLoading = true;

            if (IsEditing)
            {
                if (FormLessonId is null || string.IsNullOrEmpty(FormRowVersionBase64)) return;

                var updateRequest = new UpdateLessonTopicAndHomeworkRequest(FormLessonTopic, FormHomework, FormRowVersionBase64);
                var response = await lessonApi.UpdateTopicAndHomeworkAsync(FormLessonId.Value, updateRequest, ct).ConfigureAwait(true);

                await ProcessApiResponseAsync(response, ct).ConfigureAwait(true);
            }
            else
            {
                if (SelectedAssignmentId is null || FormLessonDate is null || SelectedLessonType is null ||
                    SelectedBellSchedule is null || SelectedClassroom is null || SelectedSemester is null)
                {
                    ErrorMessage = "Будь ласка, заповніть всі обов'язкові поля для створення уроку.";
                    return;
                }

                var createRequest = new CreateLessonRequest(
                    SelectedAssignmentId.Value,
                    new DateTimeOffset(DateTime.SpecifyKind(FormLessonDate.Value.Date, DateTimeKind.Utc)),
                    FormLessonTopic,
                    FormHomework,
                    SelectedLessonType.LessonTypeId,
                    SelectedBellSchedule.ScheduleId,
                    SelectedClassroom.RoomId,
                    SelectedSemester.SemesterId);

                var response = await lessonApi.CreateLessonAsync(createRequest, ct).ConfigureAwait(true);

                await ProcessApiResponseAsync(response, ct).ConfigureAwait(true);
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
    private void OpenRescheduleForm(LessonResponse lesson)
    {
        FormLessonId = lesson.LessonId;
        FormRowVersionBase64 = lesson.RowVersionBase64;

        RescheduleDate = lesson.LessonDate.LocalDateTime;
        RescheduleSelectedBellSchedule = BellSchedules.FirstOrDefault(b => b.ScheduleId == lesson.PeriodId);
        RescheduleSelectedClassroom = Classrooms.FirstOrDefault(c => c.RoomId == lesson.RoomId);

        ErrorMessage = null;
        IsRescheduleFormOpen = true;
    }

    [RelayCommand]
    private void CloseRescheduleForm()
    {
        IsRescheduleFormOpen = false;
        ErrorMessage = null;
    }

    [RelayCommand]
    private async Task SaveRescheduleAsync(CancellationToken ct)
    {
        if (FormLessonId is null || string.IsNullOrEmpty(FormRowVersionBase64) ||
            RescheduleDate is null || RescheduleSelectedBellSchedule is null || RescheduleSelectedClassroom is null)
        {
            ErrorMessage = "Будь ласка, оберіть нову дату, час та кабінет.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var dateUtc = new DateTimeOffset(DateTime.SpecifyKind(RescheduleDate.Value.Date, DateTimeKind.Utc));
            var request = new RescheduleLessonRequest(
                dateUtc,
                RescheduleSelectedBellSchedule.ScheduleId,
                RescheduleSelectedClassroom.RoomId,
                FormRowVersionBase64);

            var response = await lessonApi.RescheduleLessonAsync(FormLessonId.Value, request, ct).ConfigureAwait(true);

            await ProcessApiResponseAsync(response, ct).ConfigureAwait(true);
            
            if (response.IsSuccessStatusCode)
            {
                IsRescheduleFormOpen = false;
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
    private async Task DeleteLessonAsync(LessonResponse lesson, CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var request = new DeleteLessonRequest(lesson.RowVersionBase64);
            var response = await lessonApi.DeleteLessonAsync(lesson.LessonId, request, ct).ConfigureAwait(true);

            await ProcessApiResponseAsync(response, ct).ConfigureAwait(true);
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

    private async Task ProcessApiResponseAsync(IApiResponse response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            IsFormOpen = false;
            if (SelectedAssignmentId.HasValue)
            {
                await LoadLessonsAsync(SelectedAssignmentId.Value, ct).ConfigureAwait(true);
            }
        }
        else if (response.Error is not null)
        {
            var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
            var serverMessage = problem?["title"]?.ToString();

            ErrorMessage = response.StatusCode switch
            {
                System.Net.HttpStatusCode.Conflict => "Конфлікт даних (RowVersion). Хтось інший вже змінив цей урок. Оновіть список.",
                System.Net.HttpStatusCode.Forbidden => "У вас немає доступу до цієї дії.",
                System.Net.HttpStatusCode.BadRequest => serverMessage ?? "Некоректні дані запиту.",
                _ => $"Помилка API: {response.Error.Message}"
            };
        }
    }
}