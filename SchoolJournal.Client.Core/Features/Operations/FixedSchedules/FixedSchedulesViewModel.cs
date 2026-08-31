using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Client.Core.Features.Schedule;
using SchoolJournal.Contracts.DTOs.Operations.FixedSchedules;
using SchoolJournal.Contracts.Enums.Identity;
using System.Collections.ObjectModel;

namespace SchoolJournal.Client.Core.Features.Operations.FixedSchedules;

public sealed partial class FixedSchedulesViewModel : ObservableObject
{
    private readonly IFixedSchedulesApi _fixedSchedulesApi;
    private readonly IIdentityService _identityService;

    public FixedSchedulesViewModel(IFixedSchedulesApi fixedSchedulesApi, IIdentityService identityService)
    {
        _fixedSchedulesApi = fixedSchedulesApi;
        _identityService = identityService;

        IsAdminOrDirector = _identityService.IsInRole(RoleType.Admin, RoleType.Director);
        SelectedDayOfWeek = 1;

        _ = LoadSchedulesByDayAsync(CancellationToken.None);
    }

    [ObservableProperty]
    private ObservableCollection<FixedScheduleResponse> _schedules = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isAdminOrDirector;

    [ObservableProperty]
    private int _selectedDayOfWeek;

    [ObservableProperty]
    private bool _isFormOpen;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _formTitle = string.Empty;

    [ObservableProperty]
    private Guid? _formScheduleId;

    [ObservableProperty]
    private int _formDayOfWeek;

    [ObservableProperty]
    private string _formPeriodId = string.Empty;

    [ObservableProperty]
    private string _formAssignmentId = string.Empty;

    [ObservableProperty]
    private string _formRoomId = string.Empty;

    private string? _formRowVersion;

    public IReadOnlyList<int> DaysOfWeek { get; } = [1, 2, 3, 4, 5, 6, 7];

    [RelayCommand]
    private async Task LoadSchedulesByDayAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _fixedSchedulesApi.GetByDayAsync(SelectedDayOfWeek, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                Schedules = new ObservableCollection<FixedScheduleResponse>(response.Content);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження розкладу: {ex.Message}";
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
        FormTitle = "Додати урок у розклад";
        FormScheduleId = null;
        FormDayOfWeek = SelectedDayOfWeek;
        FormPeriodId = string.Empty;
        FormAssignmentId = string.Empty;
        FormRoomId = string.Empty;
        _formRowVersion = null;
        ErrorMessage = null;
        IsFormOpen = true;
    }

    [RelayCommand]
    private async Task OpenEditFormAsync(FixedScheduleResponse schedule, CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _fixedSchedulesApi.GetByIdAsync(schedule.ScheduleId, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                var freshSchedule = response.Content;
                IsEditing = true;
                FormTitle = "Редагування розкладу";
                FormScheduleId = freshSchedule.ScheduleId;
                FormDayOfWeek = freshSchedule.DayOfWeek;
                FormPeriodId = freshSchedule.PeriodId.ToString();
                FormAssignmentId = freshSchedule.AssignmentId.ToString();
                FormRoomId = freshSchedule.RoomId.ToString();
                _formRowVersion = freshSchedule.RowVersionBase64;
                IsFormOpen = true;
            }
            else
            {
                ErrorMessage = "Не вдалося завантажити актуальні дані для редагування.";
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження запису: {ex.Message}";
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
        if (!Guid.TryParse(FormPeriodId, out var periodId) ||
            !Guid.TryParse(FormAssignmentId, out var assignmentId) ||
            !Guid.TryParse(FormRoomId, out var roomId))
        {
            ErrorMessage = "ID періоду, призначення та кабінету мають бути валідними GUID.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            IApiResponse response;

            if (IsEditing && FormScheduleId.HasValue && _formRowVersion is not null)
            {
                var request = new UpdateFixedScheduleRequest(FormDayOfWeek, periodId, assignmentId, roomId, _formRowVersion);
                response = await _fixedSchedulesApi.UpdateAsync(FormScheduleId.Value, request, ct).ConfigureAwait(true);
            }
            else
            {
                var request = new CreateFixedScheduleRequest(FormDayOfWeek, periodId, assignmentId, roomId);
                response = await _fixedSchedulesApi.CreateAsync(request, ct).ConfigureAwait(true);
            }

            if (response.IsSuccessStatusCode)
            {
                IsFormOpen = false;
                await LoadSchedulesByDayAsync(ct).ConfigureAwait(true);
            }
            else if (response.Error is ApiException apiEx)
            {
                var problem = await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true);
                var serverMessage = problem?["title"]?.ToString();

                ErrorMessage = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.BadRequest => serverMessage ?? "Некоректні дані.",
                    System.Net.HttpStatusCode.Conflict => serverMessage ?? "Конфлікт розкладу (накладка кабінету або вчителя).",
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
    private async Task DeleteScheduleAsync(FixedScheduleResponse schedule, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var request = new DeleteFixedScheduleRequest(schedule.RowVersionBase64);
            var response = await _fixedSchedulesApi.DeleteAsync(schedule.ScheduleId, request, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                Schedules.Remove(schedule);
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
}