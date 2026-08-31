using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Contracts.DTOs.Operations.Grades;
using SchoolJournal.Contracts.Enums.Identity;
using System.Collections.ObjectModel;
using System.Text.Json.Nodes;
using SchoolJournal.Client.Core.Common.ViewModels;

namespace SchoolJournal.Client.Core.Features.Operations.Grades;

public sealed partial class GradesViewModel : AppViewModelBase
{
    private readonly IGradesApi _gradesApi;
    private readonly IIdentityService _identityService;

    public GradesViewModel(IGradesApi gradesApi, IIdentityService identityService)
    {
        _gradesApi = gradesApi;
        _identityService = identityService;
        CanManageGrades = _identityService.IsInRole(RoleType.Teacher, RoleType.Admin, RoleType.Director);
    }

    [ObservableProperty]
    public partial ObservableCollection<GradeResponse> Grades { get; set; } = [];

    [ObservableProperty]
    public partial Guid LessonId { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool CanManageGrades { get; set; }

    [ObservableProperty]
    public partial bool IsFormOpen { get; set; }

    [ObservableProperty]
    public partial bool IsEditing { get; set; }

    [ObservableProperty]
    public partial string FormTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial Guid? FormGradeId { get; set; }

    [ObservableProperty]
    public partial Guid FormStudentId { get; set; }

    [ObservableProperty]
    public partial string FormGradeValue { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? FormComment { get; set; }

    [ObservableProperty]
    public partial Guid FormGradeTypeId { get; set; }

    private string? _formRowVersion;

    [RelayCommand]
    public async Task LoadGradesForLessonAsync(CancellationToken ct)
    {
        if (LessonId == Guid.Empty) return;

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _gradesApi.GetGradesByLessonAsync(LessonId, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                Grades = new ObservableCollection<GradeResponse>(response.Content);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження оцінок: {ex.Message}";
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
        FormTitle = "Виставлення оцінки";
        FormGradeId = null;
        FormStudentId = Guid.Empty;
        FormGradeValue = string.Empty;
        FormComment = string.Empty;
        FormGradeTypeId = Guid.Empty;
        _formRowVersion = null;
        ErrorMessage = null;
        IsFormOpen = true;
    }

    [RelayCommand]
    private async Task OpenEditFormAsync(GradeResponse grade, CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _gradesApi.GetGradeByIdAsync(grade.GradeId, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                var freshGrade = response.Content;
                IsEditing = true;
                FormTitle = "Редагування оцінки";
                FormGradeId = freshGrade.GradeId;
                FormStudentId = freshGrade.StudentId;
                FormGradeValue = freshGrade.GradeValue;
                FormComment = freshGrade.Comment;
                FormGradeTypeId = freshGrade.GradeTypeId;
                _formRowVersion = freshGrade.RowVersionBase64;
                IsFormOpen = true;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження даних оцінки: {ex.Message}";
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
        if (string.IsNullOrWhiteSpace(FormGradeValue) || FormStudentId == Guid.Empty || FormGradeTypeId == Guid.Empty)
        {
            ErrorMessage = "Будь ласка, заповніть усі обов'язкові поля.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            IApiResponse response;

            if (IsEditing && FormGradeId.HasValue && _formRowVersion is not null)
            {
                var request = new UpdateGradeRequest(FormGradeValue, FormComment, FormGradeTypeId, _formRowVersion);
                response = await _gradesApi.UpdateGradeAsync(FormGradeId.Value, request, ct).ConfigureAwait(true);
            }
            else
            {
                var request = new CreateGradeRequest(LessonId, FormStudentId, FormGradeValue, FormComment, FormGradeTypeId);
                response = await _gradesApi.CreateGradeAsync(request, ct).ConfigureAwait(true);
            }

            if (response.IsSuccessStatusCode)
            {
                IsFormOpen = false;
                await LoadGradesForLessonAsync(ct).ConfigureAwait(true);
            }
            else if (response.Error is not null)
            {
                var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                ErrorMessage = problem?["title"]?.ToString() ?? "Помилка обробки запиту сервером.";
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
    private async Task DeleteGradeAsync(GradeResponse grade, CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var request = new DeleteGradeRequest(grade.RowVersionBase64);
            var response = await _gradesApi.DeleteGradeAsync(grade.GradeId, request, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                Grades.Remove(grade);
            }
            else if (response.Error is not null)
            {
                var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                ErrorMessage = problem?["title"]?.ToString() ?? "Помилка видалення оцінки.";
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка системи видалення: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task BoostGradeAsync(GradeResponse grade)
    {
        // Перевіряємо правило 24 годин на стороні клієнта для миттєвого фідбеку
        if ((DateTimeOffset.UtcNow - grade.CreatedAt).TotalHours > 24)
        {
            ErrorMessage = "Час вийшов! Використати LunarCoins можна лише протягом 24 годин після отримання оцінки.";
            return;
        }

        // Захист від спаму кліків через нашу нову базову ViewModel
        await ExecuteLockedAsync(async () =>
        {
            IsLoading = true;
            ErrorMessage = null;
            try
            {
                var request = new BoostGradeRequest(grade.RowVersionBase64);
                var response = await _gradesApi.BoostGradeAsync(grade.GradeId, request, CancellationToken.None).ConfigureAwait(true);

                if (response.IsSuccessStatusCode)
                {
                    await LoadGradesForLessonAsync(CancellationToken.None).ConfigureAwait(true);
                }
                else if (response.Error is not null)
                {
                    var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                    ErrorMessage = problem?["title"]?.ToString() ?? "Помилка конвертації LunarCoins.";
                }
            }
            catch (Refit.ApiException ex)
            {
                ErrorMessage = $"Помилка API (Код {ex.StatusCode}).";
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = $"Помилка мережі: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }).ConfigureAwait(true);
    }
}