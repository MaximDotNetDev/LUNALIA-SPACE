using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Contracts.DTOs.Reference.Classrooms;
using SchoolJournal.Contracts.Enums.Identity;
using System.Collections.ObjectModel;
using System.Text.Json.Nodes;

namespace SchoolJournal.Client.Core.Features.Reference.Classroom;

public sealed partial class ClassroomsViewModel : ObservableObject
{
    private readonly IClassroomApi _classroomApi;
    private readonly IIdentityService _identityService;

    public ClassroomsViewModel(IClassroomApi classroomApi, IIdentityService identityService)
    {
        _classroomApi = classroomApi;
        _identityService = identityService;

        IsAdminOrDirector = _identityService.IsInRole(RoleType.Admin, RoleType.Director);

        _ = InitializeCommand.ExecuteAsync(null);
    }

    [ObservableProperty]
    private ObservableCollection<ClassroomResponse> _activeClassrooms = [];

    [ObservableProperty]
    private ObservableCollection<ClassroomResponse> _archivedClassrooms = [];

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
    private Guid? _formRoomId;

    [ObservableProperty]
    private string _formRoomNumber = string.Empty;

    [ObservableProperty]
    private string? _formName;

    [ObservableProperty]
    private int _formCapacity = 30;

    [ObservableProperty]
    private string _searchText = string.Empty;

    private string? _formRowVersion;

    [RelayCommand]
    private async Task InitializeAsync(CancellationToken ct)
    {
        await Task.WhenAll(
            LoadActiveClassroomsAsync(ct),
            LoadArchiveAsync(ct)
        ).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task LoadActiveClassroomsAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _classroomApi.GetActiveClassroomsAsync(1, 100, string.IsNullOrWhiteSpace(SearchText) ? null : SearchText, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                ActiveClassrooms = new ObservableCollection<ClassroomResponse>(response.Content.Items);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження активних аудиторій: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadArchiveAsync(CancellationToken ct)
    {
        if (!IsAdminOrDirector) return;

        IsLoading = true;

        try
        {
            var response = await _classroomApi.GetDeletedClassroomsAsync(1, 50, null, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                ArchivedClassrooms = new ObservableCollection<ClassroomResponse>(response.Content.Items);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = "Не вдалося завантажити архів аудиторій.";
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
        FormTitle = "Створення аудиторії";
        FormRoomId = null;
        FormRoomNumber = string.Empty;
        FormName = string.Empty;
        FormCapacity = 30;
        _formRowVersion = null;
        ErrorMessage = null;
        IsFormOpen = true;
    }

    [RelayCommand]
    private async Task OpenEditFormAsync(ClassroomResponse classroom, CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _classroomApi.GetClassroomByIdAsync(classroom.RoomId, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                var freshClassroom = response.Content;
                IsEditing = true;
                FormTitle = "Редагування аудиторії";
                FormRoomId = freshClassroom.RoomId;
                FormRoomNumber = freshClassroom.RoomNumber;
                FormName = freshClassroom.Name;
                FormCapacity = freshClassroom.Capacity;
                _formRowVersion = freshClassroom.RowVersionBase64;
                IsFormOpen = true;
            }
            else
            {
                ErrorMessage = "Не вдалося завантажити актуальні дані аудиторії для редагування.";
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження аудиторії: {ex.Message}";
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
        if (string.IsNullOrWhiteSpace(FormRoomNumber))
        {
            ErrorMessage = "Номер аудиторії є обов'язковим.";
            return;
        }

        if (FormCapacity <= 0 || FormCapacity > 200)
        {
            ErrorMessage = "Місткість повинна бути від 1 до 200 осіб.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            IApiResponse response;

            if (IsEditing && FormRoomId.HasValue && _formRowVersion is not null)
            {
                var request = new UpdateClassroomRequest(FormRoomNumber.Trim(), FormName?.Trim(), FormCapacity, _formRowVersion);
                response = await _classroomApi.UpdateClassroomAsync(FormRoomId.Value, request, ct).ConfigureAwait(true);
            }
            else
            {
                var request = new CreateClassroomRequest(FormRoomNumber.Trim(), FormName?.Trim(), FormCapacity);
                response = await _classroomApi.CreateClassroomAsync(request, ct).ConfigureAwait(true);
            }

            if (response.IsSuccessStatusCode)
            {
                IsFormOpen = false;
                await LoadActiveClassroomsAsync(ct).ConfigureAwait(true);
            }
            else if (response.Error is not null)
            {
                var problem = response.Error is ApiException apiEx
                    ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true)
                    : null;
                var serverMessage = problem?["title"]?.ToString();

                ErrorMessage = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.BadRequest => serverMessage ?? "Некоректні дані аудиторії.",
                    System.Net.HttpStatusCode.Conflict => serverMessage ?? "Конфлікт даних: аудиторія вже існує або була змінена.",
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
    private async Task DeleteClassroomAsync(ClassroomResponse classroom, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var request = new DeleteClassroomRequest(classroom.RowVersionBase64);
            var response = await _classroomApi.DeleteClassroomAsync(classroom.RoomId, request, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                ActiveClassrooms.Remove(classroom);
                await LoadArchiveAsync(ct).ConfigureAwait(true);
            }
            else if (response.Error is not null)
            {
                var problem = response.Error is ApiException apiEx
                    ? await apiEx.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true)
                    : null;
                var serverMessage = problem?["title"]?.ToString();

                ErrorMessage = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Conflict => "Помилка паралельного доступу (RowVersion не збігається).",
                    System.Net.HttpStatusCode.BadRequest => serverMessage ?? "Некоректний запит на видалення.",
                    _ => $"Помилка видалення: {response.Error.Message}"
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

    [RelayCommand]
    private async Task RestoreClassroomAsync(ClassroomResponse classroom, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var request = new RestoreClassroomRequest(classroom.RowVersionBase64);
            var response = await _classroomApi.RestoreClassroomAsync(classroom.RoomId, request, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                ArchivedClassrooms.Remove(classroom);
                await LoadActiveClassroomsAsync(ct).ConfigureAwait(true);
            }
            else if (response.Error is not null)
            {
                ErrorMessage = $"Помилка відновлення: {response.Error.Message}";
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