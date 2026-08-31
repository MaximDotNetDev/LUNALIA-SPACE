using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Contracts.DTOs.Operations.TeacherSubstitutions;
using SchoolJournal.Contracts.Enums.Identity;
using System.Collections.ObjectModel;
using System.Net;
using System.Text.Json.Nodes;

namespace SchoolJournal.Client.Core.Features.Operations.TeacherSubstitution;

public sealed partial class TeacherSubstitutionsViewModel : ObservableObject
{
    private readonly ITeacherSubstitutionApi _substitutionApi;
    private readonly IIdentityService _identityService;

    public TeacherSubstitutionsViewModel(ITeacherSubstitutionApi substitutionApi, IIdentityService identityService)
    {
        _substitutionApi = substitutionApi;
        _identityService = identityService;

        IsAdminOrDirector = _identityService.IsInRole(RoleType.Admin, RoleType.Director);

        _ = InitializeCommand.ExecuteAsync(null);
    }

    [ObservableProperty]
    private ObservableCollection<TeacherSubstitutionResponse> _substitutions = [];

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
    private Guid? _formSubstitutionId;

    [ObservableProperty]
    private Guid _formAssignmentId;

    [ObservableProperty]
    private Guid _formSubstituteTeacherId;

    [ObservableProperty]
    private DateTime? _formStartDate;

    [ObservableProperty]
    private DateTime? _formEndDate;

    private string? _formRowVersion;

    [RelayCommand]
    private async Task InitializeAsync(CancellationToken ct)
    {
        await LoadActiveSubstitutionsAsync(ct).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task LoadActiveSubstitutionsAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _substitutionApi.GetActiveSubstitutionsAsync(ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                Substitutions = new ObservableCollection<TeacherSubstitutionResponse>(response.Content);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження активних замін: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadByTeacherAsync(Guid teacherId, CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _substitutionApi.GetSubstitutionsByTeacherIdAsync(teacherId, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                Substitutions = new ObservableCollection<TeacherSubstitutionResponse>(response.Content);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження замін вчителя: {ex.Message}";
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
        FormTitle = "Призначення заміни вчителя";
        FormSubstitutionId = null;
        FormAssignmentId = Guid.Empty;
        FormSubstituteTeacherId = Guid.Empty;
        FormStartDate = DateTime.Today;
        FormEndDate = DateTime.Today;
        _formRowVersion = null;
        ErrorMessage = null;
        IsFormOpen = true;
    }

    [RelayCommand]
    private async Task OpenEditFormAsync(TeacherSubstitutionResponse substitution, CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _substitutionApi.GetSubstitutionByIdAsync(substitution.SubstitutionId, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                var freshData = response.Content;
                IsEditing = true;
                FormTitle = "Редагування заміни вчителя";
                FormSubstitutionId = freshData.SubstitutionId;
                FormAssignmentId = freshData.AssignmentId;
                FormSubstituteTeacherId = freshData.SubstituteTeacherId;
                FormStartDate = freshData.StartDate.LocalDateTime;
                FormEndDate = freshData.EndDate.LocalDateTime;
                _formRowVersion = freshData.RowVersionBase64;
                IsFormOpen = true;
            }
            else
            {
                ErrorMessage = "Не вдалося завантажити актуальні дані заміни.";
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
        if (FormAssignmentId == Guid.Empty || FormSubstituteTeacherId == Guid.Empty || FormStartDate is null || FormEndDate is null)
        {
            ErrorMessage = "Будь ласка, заповніть всі обов'язкові поля.";
            return;
        }

        if (FormEndDate < FormStartDate)
        {
            ErrorMessage = "Дата закінчення не може бути ранішою за дату початку.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var startDateUtc = new DateTimeOffset(DateTime.SpecifyKind(FormStartDate.Value.Date, DateTimeKind.Utc));
            var endDateUtc = new DateTimeOffset(DateTime.SpecifyKind(FormEndDate.Value.Date, DateTimeKind.Utc));

            IApiResponse response;

            if (IsEditing && FormSubstitutionId.HasValue && _formRowVersion is not null)
            {
                var request = new UpdateTeacherSubstitutionRequest(FormAssignmentId, FormSubstituteTeacherId, startDateUtc, endDateUtc, _formRowVersion);
                response = await _substitutionApi.UpdateSubstitutionAsync(FormSubstitutionId.Value, request, ct).ConfigureAwait(true);
            }
            else
            {
                var request = new CreateTeacherSubstitutionRequest(FormAssignmentId, FormSubstituteTeacherId, startDateUtc, endDateUtc);
                response = await _substitutionApi.CreateSubstitutionAsync(request, ct).ConfigureAwait(true);
            }

            if (response.IsSuccessStatusCode)
            {
                IsFormOpen = false;
                await LoadActiveSubstitutionsAsync(ct).ConfigureAwait(true);
            }
            else if (response.Error is not null)
            {
                var problem = response.Error is ApiException apiEx
                    ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true)
                    : null;
                var serverMessage = problem?["title"]?.ToString();

                ErrorMessage = response.StatusCode switch
                {
                    HttpStatusCode.BadRequest => serverMessage ?? "Некоректні дані заміни.",
                    HttpStatusCode.Conflict => serverMessage ?? "Конфлікт паралельного доступу або перетин дат розкладу.",
                    _ => $"Помилка сервера: {response.Error.Message}"
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
    private async Task DeleteSubstitutionAsync(TeacherSubstitutionResponse substitution, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var request = new DeleteTeacherSubstitutionRequest(substitution.RowVersionBase64);
            var response = await _substitutionApi.DeleteSubstitutionAsync(substitution.SubstitutionId, request, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                Substitutions.Remove(substitution);
            }
            else if (response.Error is not null)
            {
                var problem = response.Error is ApiException apiEx
                    ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true)
                    : null;
                var serverMessage = problem?["title"]?.ToString();

                ErrorMessage = response.StatusCode switch
                {
                    HttpStatusCode.Conflict => "Запис уже змінено або видалено в базі даних.",
                    _ => $"Помилка видалення: {serverMessage ?? response.Error.Message}"
                };
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not StackOverflowException)
        {
            ErrorMessage = $"Помилка системи: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}