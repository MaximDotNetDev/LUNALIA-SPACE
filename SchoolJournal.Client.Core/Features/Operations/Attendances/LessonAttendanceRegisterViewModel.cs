using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Contracts.DTOs.Operations.Attendances;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolJournal.Client.Core.Features.Operations.Attendances;

public sealed partial class LessonAttendanceRegisterViewModel(IAttendancesApi attendancesApi) : ObservableObject
{
    private Guid _currentLessonId;

    [ObservableProperty]
    private ObservableCollection<StudentAttendanceUIModel> _students = [];

    [ObservableProperty]
    private string _lessonTopic = string.Empty;

    [ObservableProperty]
    private string _lessonDate = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    public async Task InitializeAsync(Guid lessonId)
    {
        _currentLessonId = lessonId;
        await LoadRegisterAsync(CancellationToken.None).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task LoadRegisterAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await attendancesApi.GetLessonAttendanceRegisterAsync(_currentLessonId, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                LessonTopic = response.Content.LessonTopic ?? "Тема не вказана";
                LessonDate = response.Content.LessonDate.LocalDateTime.ToString("dd.MM.yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);

                var uiModels = response.Content.Students.Select(s => new StudentAttendanceUIModel
                {
                    StudentId = s.StudentId,
                    FullName = $"{s.LastName} {s.FirstName} {s.MiddleName}".Trim(),
                    OriginalAttendanceId = s.AttendanceId,
                    Status = s.Status,
                    Comment = s.Comment
                });

                Students = new ObservableCollection<StudentAttendanceUIModel>(uiModels);
            }
            else if (response.Error is not null)
            {
var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                ErrorMessage = problem?["title"]?.ToString() ?? "Помилка завантаження відомості.";
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Мережева або системна помилка: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveBulkAttendancesAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var itemsToSave = Students
                .Where(s => !string.IsNullOrWhiteSpace(s.Status))
                .Select(s => new StudentAttendanceItem(
                    s.StudentId,
                    s.Status!.Trim(),
                    string.IsNullOrWhiteSpace(s.Comment) ? null : s.Comment.Trim()
                )).ToList();

            if (itemsToSave.Count == 0)
            {
                ErrorMessage = "Немає даних для збереження. Вкажіть статус хоча б одному учню.";
                IsLoading = false;
                return;
            }

            var request = new BulkRecordAttendanceRequest(_currentLessonId, itemsToSave);
            var response = await attendancesApi.BulkRecordAttendanceAsync(request, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                await LoadRegisterAsync(ct).ConfigureAwait(true);
            }
            else if (response.Error is not null)
            {
var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                ErrorMessage = problem?["title"]?.ToString() ?? "Помилка при збереженні відвідуваності.";
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка збереження: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}