using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Communications.Announcements;
using SchoolJournal.Contracts.Enums.Identity;
using System.Collections.ObjectModel;
using System.Reflection.Metadata;
using static System.Net.WebRequestMethods;

namespace SchoolJournal.Client.Core.Features.Communications.Announcements;

public sealed partial class AnnouncementsViewModel : ObservableObject
{
    private readonly IAnnouncementsApi _announcementsApi;
    private readonly IIdentityService _identityService;

    public AnnouncementsViewModel(IAnnouncementsApi announcementsApi, IIdentityService identityService)
    {
        _announcementsApi = announcementsApi;
        _identityService = identityService;

        IsAdminOrDirector = _identityService.IsInRole(RoleType.Admin, RoleType.Director);

        _ = InitializeCommand.ExecuteAsync(null);
    }

    [ObservableProperty]
    public partial ObservableCollection<AnnouncementResponse> Announcements { get; set; } = [];

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsAdminOrDirector { get; set; }

    [ObservableProperty]
    public partial int CurrentPage { get; set; } = 1;

    [ObservableProperty]
    public partial int PageSize { get; set; } = 10;

    [ObservableProperty]
    public partial int TotalCount { get; set; }

    [ObservableProperty] public partial bool IsFormOpen { get; set; }
    [ObservableProperty] public partial string FormTitle { get; set; } = string.Empty;
    [ObservableProperty] public partial Guid? EditingId { get; set; }
    [ObservableProperty] public partial string Title { get; set; } = string.Empty;
    [ObservableProperty] public partial string Content { get; set; } = string.Empty;
    [ObservableProperty] public partial DateTime? ExpirationDate { get; set; }

    // Kept as a standard private field because it did not use [ObservableProperty]
    private string? _rowVersion;

    [RelayCommand]
    private async Task InitializeAsync(CancellationToken ct) => await LoadAnnouncementsAsync(ct).ConfigureAwait(true);

    [RelayCommand]
    private async Task LoadAnnouncementsAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            IApiResponse<PagedResponse<AnnouncementResponse>> response;

            if (IsAdminOrDirector)
            {
                response = await _announcementsApi.GetAdminAnnouncementsAsync(CurrentPage, PageSize, null, null, null, ct).ConfigureAwait(true);
            }
            else
            {
                response = await _announcementsApi.GetActiveAnnouncementsAsync(CurrentPage, PageSize, ct).ConfigureAwait(true);
            }

            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                Announcements = new ObservableCollection<AnnouncementResponse>(response.Content.Items);
                TotalCount = response.Content.TotalCount;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = $"Помилка завантаження оголошень: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void OpenCreateForm()
    {
        EditingId = null;
        Title = string.Empty;
        Content = string.Empty;
        ExpirationDate = null;
        _rowVersion = null;
        FormTitle = "Нове оголошення";
        IsFormOpen = true;
    }

    [RelayCommand]
    private void OpenEditForm(AnnouncementResponse announcement)
    {
        EditingId = announcement.AnnouncementId;
        Title = announcement.Title;
        Content = announcement.Content;
        ExpirationDate = announcement.ExpirationDate?.LocalDateTime;
        _rowVersion = announcement.RowVersionBase64;
        FormTitle = "Редагування";
        IsFormOpen = true;
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Content)) return;

        IsLoading = true;
        try
        {
            DateTimeOffset? expDate = ExpirationDate.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(ExpirationDate.Value, DateTimeKind.Utc))
                : null;

            IApiResponse response;
            if (EditingId.HasValue && _rowVersion is not null)
            {
                var request = new UpdateAnnouncementRequest(Title, Content, expDate, _rowVersion);
                response = await _announcementsApi.UpdateAnnouncementAsync(EditingId.Value, request, ct).ConfigureAwait(true);
            }
            else
            {
                var request = new CreateAnnouncementRequest(Title, Content, expDate);
                response = await _announcementsApi.CreateAnnouncementAsync(request, ct).ConfigureAwait(true);
            }

            if (response.IsSuccessStatusCode)
            {
                IsFormOpen = false;
                await LoadAnnouncementsAsync(ct).ConfigureAwait(true);
            }
            else
            {
                ErrorMessage = "Не вдалося зберегти зміни.";
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not StackOverflowException)
        {
            ErrorMessage = $"Помилка збереження: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(AnnouncementResponse announcement, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var request = new DeleteAnnouncementRequest(announcement.RowVersionBase64);
            var response = await _announcementsApi.DeleteAnnouncementAsync(announcement.AnnouncementId, request, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode) await LoadAnnouncementsAsync(ct).ConfigureAwait(true);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ToggleStatusAsync(AnnouncementResponse announcement, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var request = new ToggleAnnouncementStatusRequest(announcement.RowVersionBase64);
            var response = await _announcementsApi.ToggleStatusAsync(announcement.AnnouncementId, request, ct).ConfigureAwait(true);
            if (response.IsSuccessStatusCode) await LoadAnnouncementsAsync(ct).ConfigureAwait(true);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void CloseForm() => IsFormOpen = false;
}