using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Client.Core.Features.Core.SchoolClasses;
using SchoolJournal.Client.Core.Features.Core.Subject;
using SchoolJournal.Contracts.DTOs.Core.SchoolClasses;
using SchoolJournal.Contracts.DTOs.Core.Subjects;
using SchoolJournal.Contracts.DTOs.Core.Subgroups;
using SchoolJournal.Contracts.Enums.Identity;
using System.Collections.ObjectModel;


namespace SchoolJournal.Client.Core.Features.Core.Subgroups;

public sealed partial class SubgroupsViewModel : ObservableObject
{
    private readonly ISubgroupsApi _subgroupsApi;
    private readonly ISchoolClassApi _schoolClassApi;
    private readonly ISubjectApi _subjectApi;
    private readonly IIdentityService _identityService;

    public SubgroupsViewModel(
        ISubgroupsApi subgroupsApi,
        ISchoolClassApi schoolClassApi,
        ISubjectApi subjectApi,
        IIdentityService identityService)
    {
        _subgroupsApi = subgroupsApi;
        _schoolClassApi = schoolClassApi;
        _subjectApi = subjectApi;
        _identityService = identityService;

        IsAdminOrDirector = _identityService.IsInRole(RoleType.Admin, RoleType.Director);

        _ = InitializeCommand.ExecuteAsync(null);
    }

    [ObservableProperty]
    private ObservableCollection<SubgroupResponse> _subgroups = [];

    [ObservableProperty]
    private ObservableCollection<SchoolClassItemResponse> _availableClasses = [];

    [ObservableProperty]
    private ObservableCollection<SubjectResponse> _availableSubjects = [];

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
    private Guid? _formSubgroupId;

    [ObservableProperty]
    private SchoolClassItemResponse? _selectedClass;

    [ObservableProperty]
    private SubjectResponse? _selectedSubject;

    [ObservableProperty]
    private string _formSubgroupName = string.Empty;

    [ObservableProperty]
    private bool _formIsActive;

    private string? _formRowVersion;

    [RelayCommand]
    private async Task InitializeAsync(CancellationToken ct)
    {
        await LoadSubgroupsAsync(ct).ConfigureAwait(true);
        if (IsAdminOrDirector)
        {
            await LoadDictionariesAsync(ct).ConfigureAwait(true);
        }
    }

    [RelayCommand]
    private async Task LoadSubgroupsAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _subgroupsApi.GetSubgroupsListAsync(1, 1000, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                Subgroups = new ObservableCollection<SubgroupResponse>(response.Content.Items);
            }
            else
            {
                ErrorMessage = "Не вдалося завантажити список підгруп.";
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

    private async Task LoadDictionariesAsync(CancellationToken ct)
    {
        try
        {
            var classesTask = _schoolClassApi.GetActiveClassesAsync(1, 100, null, ct);
            var subjectsTask = _subjectApi.GetActiveSubjectsAsync(1, 100, null, ct);

            await Task.WhenAll(classesTask, subjectsTask).ConfigureAwait(true);

            var classesResponse = await classesTask.ConfigureAwait(true);
            var subjectsResponse = await subjectsTask.ConfigureAwait(true);

            if (classesResponse.IsSuccessStatusCode && classesResponse.Content is not null)
                AvailableClasses = new ObservableCollection<SchoolClassItemResponse>(classesResponse.Content.Items);

            if (subjectsResponse.IsSuccessStatusCode && subjectsResponse.Content is not null)
                AvailableSubjects = new ObservableCollection<SubjectResponse>(subjectsResponse.Content.Items);
        }
        catch (ApiException ex)
        {
            ErrorMessage = $"Не вдалося завантажити довідники (API): {ex.Message}";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Не вдалося завантажити довідники (Мережа): {ex.Message}";
        }
    }

    [RelayCommand]
    private void OpenCreateForm()
    {
        IsEditing = false;
        FormTitle = "Створення підгрупи";
        FormSubgroupId = null;
        SelectedClass = null;
        SelectedSubject = null;
        FormSubgroupName = string.Empty;
        FormIsActive = true;
        _formRowVersion = null;
        ErrorMessage = null;
        IsFormOpen = true;
    }

    [RelayCommand]
    private async Task OpenEditFormAsync(SubgroupResponse subgroup, CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _subgroupsApi.GetSubgroupByIdAsync(subgroup.SubgroupId, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                var freshData = response.Content;
                IsEditing = true;
                FormTitle = "Редагування підгрупи";
                FormSubgroupId = freshData.SubgroupId;
                FormSubgroupName = freshData.SubgroupName;
                FormIsActive = freshData.IsActive;
                _formRowVersion = freshData.RowVersionBase64;

                SelectedClass = AvailableClasses.FirstOrDefault(c => c.ClassId == freshData.ClassId);
                SelectedSubject = AvailableSubjects.FirstOrDefault(s => s.SubjectId == freshData.SubjectId);

                IsFormOpen = true;
            }
            else
            {
                ErrorMessage = "Не вдалося завантажити актуальні дані підгрупи.";
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
    private void CloseForm()
    {
        IsFormOpen = false;
        ErrorMessage = null;
    }

    [RelayCommand]
    private async Task SaveFormAsync(CancellationToken ct)
    {
        if (SelectedClass is null || SelectedSubject is null || string.IsNullOrWhiteSpace(FormSubgroupName))
        {
            ErrorMessage = "Будь ласка, заповніть всі поля.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            IApiResponse response;

            if (IsEditing && FormSubgroupId.HasValue && _formRowVersion is not null)
            {
                var request = new UpdateSubgroupRequest(FormSubgroupName, FormIsActive, _formRowVersion);
                response = await _subgroupsApi.UpdateSubgroupAsync(FormSubgroupId.Value, request, ct).ConfigureAwait(true);
            }
            else
            {
                var request = new CreateSubgroupRequest(SelectedClass.ClassId, SelectedSubject.SubjectId, FormSubgroupName);
                response = await _subgroupsApi.CreateSubgroupAsync(request, ct).ConfigureAwait(true);
            }

            if (response.IsSuccessStatusCode)
            {
                IsFormOpen = false;
                await LoadSubgroupsAsync(ct).ConfigureAwait(true);
            }
            else if (response.Error is not null)
            {
var problem = response.Error is ApiException apiEx ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true) : null;
                var serverMessage = problem?["title"]?.ToString();

                ErrorMessage = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.BadRequest => serverMessage ?? "Некоректні дані.",
                    System.Net.HttpStatusCode.Conflict => serverMessage ?? "Конфлікт (можливо, така підгрупа вже існує).",
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
    private async Task DeleteSubgroupAsync(SubgroupResponse subgroup, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var request = new DeleteSubgroupRequest(subgroup.RowVersionBase64);
            var response = await _subgroupsApi.DeleteSubgroupAsync(subgroup.SubgroupId, request, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                Subgroups.Remove(subgroup);
            }
            else if (response.Error is not null)
            {
                ErrorMessage = $"Помилка видалення: {response.Error.Message}";
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