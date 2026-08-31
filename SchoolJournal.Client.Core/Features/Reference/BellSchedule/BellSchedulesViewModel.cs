using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Contracts.DTOs.Reference.BellSchedules;
using SchoolJournal.Contracts.Enums.Identity;
using System.Collections.ObjectModel;
using System.Text.Json.Nodes;

namespace SchoolJournal.Client.Core.Features.Reference.BellSchedule;

public sealed partial class BellSchedulesViewModel : ObservableObject
{
    private readonly IBellScheduleApi _bellScheduleApi;
    private readonly IIdentityService _identityService;

    public BellSchedulesViewModel(IBellScheduleApi bellScheduleApi, IIdentityService identityService)
    {
        _bellScheduleApi = bellScheduleApi;
        _identityService = identityService;

        IsAdminOrDirector = _identityService.IsInRole(RoleType.Admin, RoleType.Director);

        _ = InitializeCommand.ExecuteAsync(null);
    }

    [ObservableProperty]
    private ObservableCollection<BellScheduleResponse> _activeSchedules = [];

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
    private string _formTitle = string.Empty;

    [ObservableProperty]
    private Guid? _formScheduleId;

    [ObservableProperty]
    private string _formLessonNumber = string.Empty;

    [ObservableProperty]
    private TimeSpan? _formStartTime;

    [ObservableProperty]
    private TimeSpan? _formEndTime;

    [RelayCommand]
    private async Task InitializeAsync(CancellationToken ct)
    {
        await LoadActiveSchedulesAsync(ct).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task LoadActiveSchedulesAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _bellScheduleApi.GetActiveBellSchedulesAsync(1, 100, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                ActiveSchedules = new ObservableCollection<BellScheduleResponse>(response.Content.Items);
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
        FormTitle = "Створення розкладу";
        FormScheduleId = null;
        FormLessonNumber = string.Empty;
        FormStartTime = new TimeSpan(8, 0, 0);
        FormEndTime = new TimeSpan(8, 45, 0);
        ErrorMessage = null;
        IsFormOpen = true;
    }

    [RelayCommand]
    private async Task OpenEditFormAsync(BellScheduleResponse schedule, CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _bellScheduleApi.GetBellScheduleByIdAsync(schedule.ScheduleId, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                var freshData = response.Content;
                IsEditing = true;
                FormTitle = "Редагування розкладу";
                FormScheduleId = freshData.ScheduleId;
                FormLessonNumber = freshData.LessonNumber.ToString(System.Globalization.CultureInfo.InvariantCulture);
                FormStartTime = freshData.StartTime.TimeOfDay;
                FormEndTime = freshData.EndTime.TimeOfDay;
                IsFormOpen = true;
            }
            else
            {
                ErrorMessage = "Не вдалося завантажити актуальні дані для редагування.";
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

    [RelayCommand]
    private void CloseForm()
    {
        IsFormOpen = false;
        ErrorMessage = null;
    }

    [RelayCommand]
    private async Task SaveFormAsync(CancellationToken ct)
    {
        _ = int.TryParse(FormLessonNumber, out int lessonNum);
        if (lessonNum <= 0)
        {
            ErrorMessage = "Будь ласка, введіть коректний номер уроку (більше 0).";
            return;
        }

        if (FormStartTime is null || FormEndTime is null)
        {
            ErrorMessage = "Будь ласка, заповніть час початку та завершення.";
            return;
        }

        if (FormEndTime <= FormStartTime)
        {
            ErrorMessage = "Час завершення має бути пізнішим за час початку.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            // Формуємо DateTimeOffset із TimeSpan, використовуючи поточну дату (API перевіряє лише час доби)
            var today = DateTime.UtcNow.Date;
            var startDateTimeOffset = new DateTimeOffset(today.Add(FormStartTime.Value), TimeSpan.Zero);
            var endDateTimeOffset = new DateTimeOffset(today.Add(FormEndTime.Value), TimeSpan.Zero);

            IApiResponse response;

            if (IsEditing && FormScheduleId.HasValue)
            {
                var request = new UpdateBellScheduleRequest(lessonNum, startDateTimeOffset, endDateTimeOffset);
                response = await _bellScheduleApi.UpdateBellScheduleAsync(FormScheduleId.Value, request, ct).ConfigureAwait(true);
            }
            else
            {
                var request = new CreateBellScheduleRequest(lessonNum, startDateTimeOffset, endDateTimeOffset);
                response = await _bellScheduleApi.CreateBellScheduleAsync(request, ct).ConfigureAwait(true);
            }

            if (response.IsSuccessStatusCode)
            {
                IsFormOpen = false;
                await LoadActiveSchedulesAsync(ct).ConfigureAwait(true);
            }
            else if (response.Error is not null)
            {
var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                var serverMessage = problem?["title"]?.ToString();

                ErrorMessage = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.BadRequest => serverMessage ?? "Некоректні дані.",
                    System.Net.HttpStatusCode.Conflict => serverMessage ?? "Конфлікт даних: такий урок або час вже існує.",
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
    private async Task DeleteScheduleAsync(BellScheduleResponse schedule, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var response = await _bellScheduleApi.DeleteBellScheduleAsync(schedule.ScheduleId, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                ActiveSchedules.Remove(schedule);
            }
            else if (response.Error is not null)
            {
                var problem = response.Error is ApiException apiEx
                    ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true)
                    : null;
                var serverMessage = problem?["title"]?.ToString();

                ErrorMessage = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.NotFound => "Розклад вже видалено іншим користувачем.",
                    _ => $"Помилка видалення: {serverMessage ?? response.Error.Message}"
                };
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