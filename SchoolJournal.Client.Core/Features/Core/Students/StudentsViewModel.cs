using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Client.Core.Features.Core.SchoolClasses;
using SchoolJournal.Client.Core.Features.Identity.Common;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Core.SchoolClasses;
using SchoolJournal.Contracts.DTOs.Core.Students;
using SchoolJournal.Contracts.Enums.Identity;
using SchoolJournal.Contracts.DTOs.Identity.Register;
using System.Collections.ObjectModel;

namespace SchoolJournal.Client.Core.Features.Core.Students;

public sealed partial class StudentsViewModel : ObservableObject
{
    private readonly IStudentApi _studentApi;
    private readonly IIdentityService _identityService;
    private readonly IIdentityApi _identityApi;
    private readonly ISchoolClassApi _schoolClassApi; // Додано для отримання класів

    public StudentsViewModel(IStudentApi studentApi, IIdentityService identityService, IIdentityApi identityApi, ISchoolClassApi schoolClassApi)
    {
        _studentApi = studentApi;
        _identityService = identityService;
        _identityApi = identityApi;
        _schoolClassApi = schoolClassApi;

        IsAdminOrDirector = _identityService.IsInRole(RoleType.Admin, RoleType.Director);
        _ = LoadClassesAsync();
        _ = SearchAsync(default);
    }

    [ObservableProperty] public partial ObservableCollection<StudentSearchResponse> Students { get; set; } = [];
    [ObservableProperty] public partial ObservableCollection<StudentHistoryResponse> HistoryRecords { get; set; } = [];
    [ObservableProperty] public partial ObservableCollection<SchoolClassItemResponse> AvailableClasses { get; set; } = [];
    [ObservableProperty] public partial bool IsLoading { get; set; }
    [ObservableProperty] public partial string? ErrorMessage { get; set; }
    [ObservableProperty] public partial bool IsAdminOrDirector { get; set; }

    [ObservableProperty] public partial string? SearchTerm { get; set; }
    [ObservableProperty] public partial Guid? FilterClassId { get; set; } // ДОДАНО: Фільтр по класу
    [ObservableProperty] public partial int PageNumber { get; set; } = 1;
    [ObservableProperty] public partial int TotalCount { get; set; }

    // Миттєвий пошук при зміні класу
    partial void OnFilterClassIdChanged(Guid? value) => _ = SearchAsync(default);

    // UI States
    [ObservableProperty] public partial bool IsFormOpen { get; set; }
    [ObservableProperty] public partial bool IsHistoryOpen { get; set; }
    [ObservableProperty] public partial string FormTitle { get; set; } = string.Empty;
    [ObservableProperty] public partial bool IsEditing { get; set; }

    // Form Fields
    [ObservableProperty] public partial Guid? FormStudentId { get; set; }
    [ObservableProperty] public partial string LastName { get; set; } = string.Empty;
    [ObservableProperty] public partial string FirstName { get; set; } = string.Empty;
    [ObservableProperty] public partial string? MiddleName { get; set; }
    [ObservableProperty] public partial DateTime? DateOfBirth { get; set; }
    [ObservableProperty] public partial Guid ClassId { get; set; }
    [ObservableProperty] public partial string? Gender { get; set; }
    [ObservableProperty] public partial string? DocumentType { get; set; }
    [ObservableProperty] public partial string? DocumentNumber { get; set; }
    [ObservableProperty] public partial string? Address { get; set; }
    [ObservableProperty] public partial string? MedicalNotes { get; set; }

    private string? _rowVersion;

    // ОБЛІКОВИЙ ЗАПИС
    [ObservableProperty] public partial bool IsAccountFormOpen { get; set; }
    [ObservableProperty] public partial string AccountLogin { get; set; } = string.Empty;
    [ObservableProperty] public partial string AccountPassword { get; set; } = string.Empty;
    private Guid? _accountStudentId;

    // ПЕРЕВЕДЕННЯ УЧНЯ
    [ObservableProperty] public partial bool IsTransferFormOpen { get; set; }
    [ObservableProperty] public partial Guid? TransferClassId { get; set; }
    private Guid? _transferStudentId;
    private string? _transferRowVersion;

    private async Task LoadClassesAsync()
    {
        try
        {
            var response = await _schoolClassApi.GetActiveClassesAsync(1, 1000, null, CancellationToken.None).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
                AvailableClasses = new ObservableCollection<SchoolClassItemResponse>(response.Content.Items);
        }
        catch (ApiException)
        {
            // Ігноруємо помилки сервера при завантаженні довідників
        }
        catch (HttpRequestException)
        {
            // Ігноруємо відсутність інтернету при завантаженні довідників
        }
    }

    [RelayCommand]
    private async Task SearchAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var response = await _studentApi.SearchStudentsAsync(SearchTerm, FilterClassId, true, PageNumber, 100, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                Students = new ObservableCollection<StudentSearchResponse>(response.Content.Items);
                TotalCount = response.Content.TotalCount;
            }
        }
        catch (OperationCanceledException)
        {
            // Безпечно ігноруємо скасування запиту, оскільки користувач почав новий пошук 
        }
        catch (ApiException apiEx) { ErrorMessage = $"Помилка: {apiEx.Message}"; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void OpenCreateForm()
    {
        IsEditing = false;
        FormTitle = "Реєстрація учня";
        ResetForm();
        IsFormOpen = true;
    }

    [RelayCommand]
    private async Task OpenEditFormAsync(StudentSearchResponse student, CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var response = await _studentApi.GetStudentByIdAsync(student.StudentId, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                var s = response.Content;
                IsEditing = true;
                FormTitle = $"Редагування: {s.LastName}";
                FormStudentId = s.StudentId;
                LastName = s.LastName;
                FirstName = s.FirstName;
                MiddleName = s.MiddleName;
                DateOfBirth = s.DateOfBirth?.LocalDateTime;
                ClassId = s.ClassId;
                Gender = s.Gender;
                DocumentType = s.DocumentType;
                DocumentNumber = s.DocumentNumber;
                Address = s.Address;
                MedicalNotes = s.MedicalNotes;
                _rowVersion = s.RowVersionBase64;
                IsFormOpen = true;
            }
        }
        catch (ApiException apiEx) { ErrorMessage = $"Помилка: {apiEx.Message}"; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(FirstName))
        {
            ErrorMessage = "Прізвище та ім'я є обов'язковими."; return;
        }

        IsLoading = true;
        try
        {
            IApiResponse response;
            if (IsEditing && FormStudentId.HasValue && _rowVersion is not null)
            {
                var req = new UpdateStudentRequest(LastName, FirstName, MiddleName, DateOfBirth, ClassId, Gender, DocumentType, null, DocumentNumber, null, null, Address, MedicalNotes, _rowVersion);
                response = await _studentApi.UpdateStudentAsync(FormStudentId.Value, req, ct).ConfigureAwait(true);
            }
            else
            {
                var req = new CreateStudentRequest(LastName, FirstName, MiddleName, DateOfBirth, ClassId, Gender, DocumentType, null, DocumentNumber, null, null, Address, MedicalNotes, null);
                response = await _studentApi.CreateStudentAsync(req, ct).ConfigureAwait(true);
            }

            if (response.IsSuccessStatusCode)
            {
                IsFormOpen = false;
                await SearchAsync(ct).ConfigureAwait(true);
            }
            else ErrorMessage = "Помилка збереження.";
        }
        catch (ApiException apiEx) { ErrorMessage = $"Помилка сервера: {apiEx.Message}"; }
        finally { IsLoading = false; }
    }

    [RelayCommand] private void CloseForm() => IsFormOpen = false;

    // --- ІСТОРІЯ ---
    [RelayCommand]
    private async Task ShowHistoryAsync(StudentSearchResponse student, CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var response = await _studentApi.GetStudentHistoryAsync(student.StudentId, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                HistoryRecords = new ObservableCollection<StudentHistoryResponse>(response.Content);
                IsHistoryOpen = true;
            }
            else ErrorMessage = "Не вдалося завантажити історію.";
        }
        catch (ApiException ex) { ErrorMessage = $"Помилка: {ex.StatusCode}"; }
        finally { IsLoading = false; }
    }

    [RelayCommand] private void CloseHistory() => IsHistoryOpen = false;

    // --- ПЕРЕВЕДЕННЯ ---
    [RelayCommand]
    private async Task OpenTransferFormAsync(StudentSearchResponse student, CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var detail = await _studentApi.GetStudentByIdAsync(student.StudentId, ct).ConfigureAwait(true);
            if (detail.IsSuccessStatusCode && detail.Content is not null)
            {
                _transferStudentId = student.StudentId;
                _transferRowVersion = detail.Content.RowVersionBase64;
                TransferClassId = detail.Content.ClassId;
                IsTransferFormOpen = true;
            }
        }
        catch (ApiException ex) { ErrorMessage = $"Помилка: {ex.StatusCode}"; }
        finally { IsLoading = false; }
    }

    [RelayCommand] private void CloseTransferForm() => IsTransferFormOpen = false;

    [RelayCommand]
    private async Task SaveTransferAsync(CancellationToken ct)
    {
        if (_transferStudentId is null || _transferRowVersion is null || TransferClassId is null) return;
        IsLoading = true;
        try
        {
            var request = new TransferStudentRequest(TransferClassId.Value, _transferRowVersion);
            var response = await _studentApi.TransferStudentAsync(_transferStudentId.Value, request, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode)
            {
                IsTransferFormOpen = false;
                await SearchAsync(ct).ConfigureAwait(true);
            }
            else ErrorMessage = "Помилка переведення.";
        }
        catch (ApiException ex) { ErrorMessage = $"Помилка: {ex.StatusCode}"; }
        finally { IsLoading = false; }
    }

    private void ResetForm()
    {
        FormStudentId = null; LastName = string.Empty; FirstName = string.Empty; MiddleName = null;
        DateOfBirth = DateTime.Today.AddYears(-7); Gender = "Male"; DocumentType = "BirthCertificate";
        DocumentNumber = string.Empty; Address = null; MedicalNotes = null; _rowVersion = null; ErrorMessage = null;
    }

    // --- АКАУНТ ---
    [RelayCommand]
    private void OpenAccountForm(StudentSearchResponse student)
    {
        _accountStudentId = student.StudentId;
        AccountLogin = string.Empty; AccountPassword = string.Empty; ErrorMessage = null; IsAccountFormOpen = true;
    }

    [RelayCommand] private void CloseAccountForm() => IsAccountFormOpen = false;

    [RelayCommand]
    private async Task SaveAccountAsync(CancellationToken ct)
    {
        if (_accountStudentId is null) return;
        if (string.IsNullOrWhiteSpace(AccountLogin) || string.IsNullOrWhiteSpace(AccountPassword))
        {
            ErrorMessage = "Логін та пароль є обов'язковими для реєстрації!"; return;
        }

        IsLoading = true; ErrorMessage = null;
        try
        {
            var studentDetail = await _studentApi.GetStudentByIdAsync(_accountStudentId.Value, ct).ConfigureAwait(true);
            if (!studentDetail.IsSuccessStatusCode || studentDetail.Content is null) { ErrorMessage = "Помилка бази даних."; return; }

            var newUserId = await RegisterStudentAccountAsync(AccountLogin, AccountPassword, ct).ConfigureAwait(true);
            if (newUserId is null) return;

            var linkRequest = new LinkUserToStudentRequest(newUserId.Value, studentDetail.Content.RowVersionBase64);
            var linkResponse = await _studentApi.LinkUserToStudentAsync(_accountStudentId.Value, linkRequest, ct).ConfigureAwait(true);

            if (linkResponse.IsSuccessStatusCode)
            {
                IsAccountFormOpen = false; ErrorMessage = null; await SearchAsync(ct).ConfigureAwait(true);
            }
            else ErrorMessage = "Помилка прив'язки акаунта.";
        }
        catch (ApiException ex) { ErrorMessage = $"Помилка API: {ex.StatusCode}"; }
        catch (HttpRequestException ex) { ErrorMessage = $"Помилка мережі: {ex.Message}"; }
        finally { IsLoading = false; }
    }

    private async Task<Guid?> RegisterStudentAccountAsync(string login, string password, CancellationToken ct)
    {
        var registerRequest = new RegisterRequest(login, password, RoleType.Student);
        var registerResponse = await _identityApi.RegisterAsync(registerRequest, ct).ConfigureAwait(true);
        if (registerResponse.IsSuccessStatusCode && registerResponse.Content is not null) return registerResponse.Content.UserId;

        string errorDetail = registerResponse.StatusCode?.ToString() ?? "Unknown Error";
        if (registerResponse.Error is ApiException error)
        {
            try { var problem = await error.GetContentAsAsync<System.Text.Json.Nodes.JsonObject>().ConfigureAwait(true); errorDetail = problem?["title"]?.ToString() ?? errorDetail; }
            catch (System.Text.Json.JsonException)
            {
                // Безпечно ігноруємо помилки парсингу, якщо сервер повернув пошкоджений JSON або HTML
            }
        }
        ErrorMessage = $"Помилка створення: {errorDetail}"; return null;
    }
}