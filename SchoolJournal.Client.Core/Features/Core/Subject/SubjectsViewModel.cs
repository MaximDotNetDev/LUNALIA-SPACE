using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Contracts.DTOs.Core.Subjects;
using SchoolJournal.Contracts.Enums.Identity;

namespace SchoolJournal.Client.Core.Features.Core.Subject;

public sealed partial class SubjectsViewModel : ObservableObject
{
    private readonly ISubjectApi _subjectApi;
    private readonly IIdentityService _identityService;

    public SubjectsViewModel(ISubjectApi subjectApi, IIdentityService identityService)
    {
        _subjectApi = subjectApi;
        _identityService = identityService;

        IsAdminOrDirector = _identityService.IsInRole(RoleType.Admin, RoleType.Director);

        _ = InitializeCommand.ExecuteAsync(null);
    }

    [ObservableProperty]
    private ObservableCollection<SubjectResponse> _activeSubjects = [];

    [ObservableProperty]
    private ObservableCollection<SubjectResponse> _archivedSubjects = [];

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
    private Guid? _formSubjectId;

    [ObservableProperty]
    private string _formSubjectName = string.Empty;

    [RelayCommand]
    private async Task InitializeAsync(CancellationToken ct)
    {
        await Task.WhenAll(
            LoadActiveSubjectsAsync(ct),
            LoadArchiveAsync(ct)
        ).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task LoadActiveSubjectsAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var response = await _subjectApi.GetActiveSubjectsAsync(1, 100, null, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                ActiveSubjects = new ObservableCollection<SubjectResponse>(response.Content.Items);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження предметів: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task LoadArchiveAsync(CancellationToken ct)
    {
        if (!IsAdminOrDirector) return;
        IsLoading = true;
        try
        {
            var response = await _subjectApi.GetDeletedSubjectsAsync(1, 100, null, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                ArchivedSubjects = new ObservableCollection<SubjectResponse>(response.Content.Items);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = "Не вдалося завантажити архів предметів.";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void OpenCreateForm()
    {
        IsEditing = false;
        FormTitle = "Створення предмета";
        FormSubjectId = null;
        FormSubjectName = string.Empty;
        ErrorMessage = null;
        IsFormOpen = true;
    }

    [RelayCommand]
    private void OpenEditForm(SubjectResponse subject)
    {
        IsEditing = true;
        FormTitle = "Редагування предмета";
        FormSubjectId = subject.SubjectId;
        FormSubjectName = subject.SubjectName;
        ErrorMessage = null;
        IsFormOpen = true;
    }

    [RelayCommand]
    private void CloseForm() => IsFormOpen = false;

    [RelayCommand]
    private async Task SaveFormAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(FormSubjectName))
        {
            ErrorMessage = "Назва предмета обов'язкова.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;
        try
        {
            IApiResponse response = IsEditing && FormSubjectId.HasValue
                ? await _subjectApi.UpdateSubjectAsync(FormSubjectId.Value, new UpdateSubjectRequest(FormSubjectName), ct).ConfigureAwait(true)
                : await _subjectApi.CreateSubjectAsync(new CreateSubjectRequest(FormSubjectName), ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                IsFormOpen = false;
                await LoadActiveSubjectsAsync(ct).ConfigureAwait(true);
            }
            else
            {
                ErrorMessage = "Помилка збереження. Перевірте унікальність назви.";
            }
        }
        catch (OperationCanceledException)
        {
            ErrorMessage = "Запит було скасовано.";
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not StackOverflowException and not AccessViolationException)
        {
            ErrorMessage = $"Системна помилка: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteSubjectAsync(SubjectResponse subject, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var response = await _subjectApi.DeleteSubjectAsync(subject.SubjectId, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode)
            {
                ActiveSubjects.Remove(subject);
                await LoadArchiveAsync(ct).ConfigureAwait(true);
            }
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task RestoreSubjectAsync(SubjectResponse subject, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var response = await _subjectApi.RestoreSubjectAsync(subject.SubjectId, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode)
            {
                ArchivedSubjects.Remove(subject);
                await LoadActiveSubjectsAsync(ct).ConfigureAwait(true);
            }
        }
        finally { IsLoading = false; }
    }
}