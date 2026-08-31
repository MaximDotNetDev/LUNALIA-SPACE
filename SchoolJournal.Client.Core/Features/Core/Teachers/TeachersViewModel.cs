using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Client.Core.Common.Auth;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Client.Core.Features.Reference.PedagogicalTitle;
using SchoolJournal.Client.Core.Features.Reference.Position;
using SchoolJournal.Client.Core.Features.Reference.Qualification;
using SchoolJournal.Contracts.DTOs.Core.Teachers;
using SchoolJournal.Contracts.DTOs.Reference.PedagogicalTitles;
using SchoolJournal.Contracts.DTOs.Reference.Positions;
using SchoolJournal.Contracts.DTOs.Reference.Qualifications;
using SchoolJournal.Contracts.Enums.Identity;
using System.Collections.ObjectModel;
using SchoolJournal.Client.Core.Features.Identity.Common;
using SchoolJournal.Contracts.DTOs.Identity.Register;

namespace SchoolJournal.Client.Core.Features.Core.Teachers;

public sealed partial class TeachersViewModel : ObservableObject
{
    private readonly ITeacherApi _teacherApi;
    private readonly IPositionApi _positionApi;
    private readonly IQualificationApi _qualificationApi;
    private readonly IPedagogicalTitleApi _titleApi;
    private readonly IIdentityService _identityService;
    private readonly IIdentityApi _identityApi;

    public TeachersViewModel(
        ITeacherApi teacherApi,
        IPositionApi positionApi,
        IQualificationApi qualificationApi,
        IPedagogicalTitleApi titleApi,
        IIdentityService identityService,
        IIdentityApi identityApi)
    {
        _teacherApi = teacherApi;
        _positionApi = positionApi;
        _qualificationApi = qualificationApi;
        _titleApi = titleApi;
        _identityService = identityService;
        _identityApi = identityApi;

        IsAdminOrDirector = _identityService.IsInRole(RoleType.Admin, RoleType.Director);
        _ = InitializeCommand.ExecuteAsync(null);
    }

    [ObservableProperty] public partial ObservableCollection<TeacherListItemResponse> Teachers { get; set; } = [];
    [ObservableProperty] public partial ObservableCollection<PositionResponse> Positions { get; set; } = [];
    [ObservableProperty] public partial ObservableCollection<QualificationResponse> Qualifications { get; set; } = [];
    [ObservableProperty] public partial ObservableCollection<PedagogicalTitleResponse> PedagogicalTitles { get; set; } = [];

    [ObservableProperty] public partial string? SearchQuery { get; set; }
    [ObservableProperty] public partial Guid? FilterPositionId { get; set; }

    [ObservableProperty] public partial bool IsLoading { get; set; }
    [ObservableProperty] public partial string? ErrorMessage { get; set; }
    [ObservableProperty] public partial bool IsAdminOrDirector { get; set; }
    [ObservableProperty] public partial bool IsFormOpen { get; set; }

    [ObservableProperty] public partial bool IsAssignFormOpen { get; set; }
    [ObservableProperty] public partial string AssignLoginInput { get; set; } = string.Empty;
    [ObservableProperty] public partial string AssignPasswordInput { get; set; } = string.Empty;

    // Це поле залишається приватним, бо воно не прив'язується до UI (не має ObservableProperty)
    private TeacherListItemResponse? _selectedTeacherForAssign;

    [ObservableProperty] public partial bool IsEditAccountFormOpen { get; set; }
    [ObservableProperty] public partial string EditLoginInput { get; set; } = string.Empty;
    [ObservableProperty] public partial string EditPasswordInput { get; set; } = string.Empty;
    private TeacherListItemResponse? _selectedTeacherForEdit;

    [ObservableProperty] public partial string FormLastName { get; set; } = string.Empty;
    [ObservableProperty] public partial string FormFirstName { get; set; } = string.Empty;
    [ObservableProperty] public partial string? FormMiddleName { get; set; }
    [ObservableProperty] public partial string FormGender { get; set; } = "Male";
    [ObservableProperty] public partial DateTime? FormDateOfBirth { get; set; }

    [ObservableProperty] public partial bool IsMaleChecked { get; set; } = true;
    [ObservableProperty] public partial bool IsFemaleChecked { get; set; }

    partial void OnIsMaleCheckedChanged(bool value)
    {
        if (value) FormGender = "Male";
    }

    partial void OnIsFemaleCheckedChanged(bool value)
    {
        if (value) FormGender = "Female";
    }

    [ObservableProperty] public partial Guid? FormPositionId { get; set; }
    [ObservableProperty] public partial Guid? FormQualificationId { get; set; }

    partial void OnSearchQueryChanged(string? value) => _ = LoadTeachersAsync(CancellationToken.None);
    partial void OnFilterPositionIdChanged(Guid? value) => _ = LoadTeachersAsync(CancellationToken.None);

    [RelayCommand]
    private async Task InitializeAsync(CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var posTask = _positionApi.GetPositionsAsync(1, 100, ct);
            var qualTask = _qualificationApi.GetActiveQualificationsAsync(1, 100, ct);
            var titlesTask = _titleApi.GetActivePagedAsync(1, 100, ct);

            await Task.WhenAll(posTask, qualTask, titlesTask).ConfigureAwait(true);

            var posResponse = await posTask.ConfigureAwait(true);
            var qualResponse = await qualTask.ConfigureAwait(true);
            var titlesResponse = await titlesTask.ConfigureAwait(true);

            if (posResponse.IsSuccessStatusCode && posResponse.Content is not null)
            {
                Positions = new ObservableCollection<PositionResponse>(posResponse.Content.Items);
            }
            if (qualResponse.IsSuccessStatusCode && qualResponse.Content is not null)
            {
                Qualifications = new ObservableCollection<QualificationResponse>(qualResponse.Content.Items);
            }
            if (titlesResponse.IsSuccessStatusCode && titlesResponse.Content is not null)
            {
                PedagogicalTitles = new ObservableCollection<PedagogicalTitleResponse>(titlesResponse.Content.Items);
            }

            await LoadTeachersAsync(ct).ConfigureAwait(true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException and not OutOfMemoryException)
        {
            ErrorMessage = "Помилка завантаження довідників: " + ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadTeachersAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var response = await _teacherApi.GetTeachersAsync(1, 100, SearchQuery, FilterPositionId, null, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                Teachers = new ObservableCollection<TeacherListItemResponse>(response.Content.Items);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException and not OutOfMemoryException)
        {
            ErrorMessage = $"Помилка завантаження вчителів: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void OpenCreateForm()
    {
        FormLastName = string.Empty;
        FormFirstName = string.Empty;
        FormMiddleName = null;
        FormDateOfBirth = null;
        FormGender = "Male";
        FormPositionId = null;
        FormQualificationId = null;
        ErrorMessage = null;
        IsFormOpen = true;
    }

    [RelayCommand]
    private void CloseForm() => IsFormOpen = false;

    [RelayCommand]
    private async Task SaveFormAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(FormLastName) || string.IsNullOrWhiteSpace(FormFirstName) || FormPositionId is null || FormQualificationId is null)
        {
            ErrorMessage = "Заповніть обов'язкові поля (Прізвище, Ім'я, Посада, Кваліфікація).";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var request = new CreateTeacherRequest(
                            FormLastName,
                            FormFirstName,
                            FormMiddleName,
                            null!, null!,
                            FormDateOfBirth.HasValue ? new DateTimeOffset(FormDateOfBirth.Value, TimeSpan.Zero) : null,
                            FormGender,
                            null!, null!, null!,
                            FormPositionId.Value,
                            FormQualificationId.Value,
                            null!, null!);

            var response = await _teacherApi.CreateTeacherAsync(request, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                IsFormOpen = false;
                await LoadTeachersAsync(ct).ConfigureAwait(true);
            }
            else
            {
                ErrorMessage = "Не вдалося створити профіль. Перевірте дані.";
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException and not OutOfMemoryException)
        {
            ErrorMessage = "Системна помилка: " + ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void OpenEditAccountForm(TeacherListItemResponse? teacher)
    {
        if (teacher is null || teacher.UserId is null) return;

        _selectedTeacherForEdit = teacher;
        EditLoginInput = teacher.Login ?? string.Empty; // Автоматично підставляємо поточний логін
        EditPasswordInput = string.Empty;               // Пароль завжди порожній
        ErrorMessage = null;
        IsEditAccountFormOpen = true;
    }

    [RelayCommand]
    private void CloseEditAccountForm() => IsEditAccountFormOpen = false;

    [RelayCommand]
    private async Task SaveEditAccountFormAsync(CancellationToken ct)
    {
        if (_selectedTeacherForEdit?.UserId is null) return;

        if (string.IsNullOrWhiteSpace(EditLoginInput))
        {
            ErrorMessage = "Логін не може бути порожнім.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var request = new SchoolJournal.Contracts.DTOs.Identity.UpdateAccount.UpdateAccountRequest(EditLoginInput, EditPasswordInput);
            var response = await _identityApi.UpdateAccountAsync(_selectedTeacherForEdit.UserId.Value, request, ct).ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                IsEditAccountFormOpen = false;
                ErrorMessage = null;
                await LoadTeachersAsync(ct).ConfigureAwait(true); // Оновлюємо таблицю, щоб побачити новий логін
            }
            else
            {
                string errorDetail = response.StatusCode?.ToString() ?? "Unknown Error";
                if (response.Error is ApiException error)
                {
                    try
                    {
                        var problem = await error.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true);
                        errorDetail = problem?["title"]?.ToString() ?? errorDetail;
                    }
                    catch (System.Text.Json.JsonException)
                    {
                        // Ігноруємо помилки парсингу, якщо сервер повернув не JSON
                    }
                }
                ErrorMessage = $"Помилка оновлення: {errorDetail}";
            }
        }
        catch (ApiException ex)
        {
            ErrorMessage = $"Помилка API: {ex.StatusCode}";
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
    private void OpenAssignForm(TeacherListItemResponse? teacher)
    {
        if (teacher is null) return;
        _selectedTeacherForAssign = teacher;
        AssignLoginInput = string.Empty;
        AssignPasswordInput = string.Empty;
        ErrorMessage = null;
        IsAssignFormOpen = true;
    }

    [RelayCommand]
    private void CloseAssignForm() => IsAssignFormOpen = false;

    [RelayCommand]
    private async Task SaveAssignFormAsync(CancellationToken ct)
    {
        if (_selectedTeacherForAssign is null) return;

        if (string.IsNullOrWhiteSpace(AssignLoginInput) || string.IsNullOrWhiteSpace(AssignPasswordInput))
        {
            ErrorMessage = "Заповніть логін та пароль.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            // 1. Отримуємо повний профіль, щоб мати актуальний RowVersionBase64
            var teacherDetail = await _teacherApi.GetTeacherByIdAsync(_selectedTeacherForAssign.TeacherId, ct).ConfigureAwait(true);
            if (!teacherDetail.IsSuccessStatusCode || teacherDetail.Content is null)
            {
                ErrorMessage = "Не вдалося отримати деталі профілю вчителя.";
                return;
            }

            // 2. Створюємо нового користувача в Identity (отримуємо його новий UserId)
            var registerRequest = new SchoolJournal.Contracts.DTOs.Identity.Register.RegisterRequest(AssignLoginInput, AssignPasswordInput, RoleType.Teacher);
            var registerResponse = await _identityApi.RegisterAsync(registerRequest, ct).ConfigureAwait(true);

            if (!registerResponse.IsSuccessStatusCode || registerResponse.Content is null)
            {
                string errorDetail = registerResponse.StatusCode?.ToString() ?? "Unknown Error";
                if (registerResponse.Error is ApiException error)
                {
                    try
                    {
                        var problem = await error.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true);
                        errorDetail = problem?["title"]?.ToString() ?? errorDetail;
                    }
                    catch (System.Text.Json.JsonException)
                    {
                        // Безпечно ігноруємо виключно помилки парсингу пошкодженого JSON, залишаючи стандартний статус-код
                    }
                }

                ErrorMessage = $"Помилка створення акаунта: {errorDetail}";
                return;
            }

            Guid newUserId = registerResponse.Content.UserId;

            // 3. Відправляємо PATCH запит для прив'язки нового UserId до вчителя
            var assignRequest = new AssignTeacherUserRequest(newUserId, teacherDetail.Content.RowVersionBase64);
            var assignResponse = await _teacherApi.AssignTeacherUserAsync(_selectedTeacherForAssign.TeacherId, assignRequest, ct).ConfigureAwait(true);

            if (assignResponse.IsSuccessStatusCode)
            {
                IsAssignFormOpen = false;
                ErrorMessage = null;
                await LoadTeachersAsync(ct).ConfigureAwait(true);
            }
            else
            {
                ErrorMessage = $"Помилка прив'язки створеного акаунта до профілю. Код: {assignResponse.StatusCode}";
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
}