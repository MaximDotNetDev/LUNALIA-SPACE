using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Client.Core.Features.Infrastructure.Logs;
using SchoolJournal.Contracts.Enums.Identity;
using SchoolJournal.Client.Core.Features.Settings;
using SchoolJournal.Client.Core.Features.Identity.Common;
using SchoolJournal.Client.Core.Features.Journal;
using SchoolJournal.Client.Core.Features.Academics;
using SchoolJournal.Client.Core.Features.People;
using SchoolJournal.Client.Core.Features.Schedule;
using SchoolJournal.Client.Core.Features.Administration;
using SchoolJournal.Client.Core.Features.Testing;
using SchoolJournal.Client.Core.Features.Communications.Announcements;
using SchoolJournal.Client.Core.Common.Interfaces;

namespace SchoolJournal.Client.Core.Features.Shell;

public sealed partial class MainViewModel(
    IIdentityService identityService,
    IServiceProvider serviceProvider,
    IAuthService authService,
    IUserThemeService themeService) : ObservableObject
{
    [ObservableProperty]
    public partial ObservableObject? CurrentView { get; set; }

    [ObservableProperty]
    public partial string SelectedTheme { get; set; } = themeService.CurrentTheme;

    public System.Collections.Generic.IReadOnlyList<string> AvailableThemes { get; } = ["Light", "Cosmic", "Calm", "Green"];

    partial void OnSelectedThemeChanged(string value)
    {
        // Якщо нова тема відрізняється від поточної — застосовуємо
        if (!string.IsNullOrWhiteSpace(value) && value != themeService.CurrentTheme)
        {
            _ = themeService.ApplyAndSaveThemeAsync(value);
        }
    }

    public bool IsAdmin => identityService.IsInRole(RoleType.Admin);

    public bool IsAdminOrDirector => identityService.IsInRole(RoleType.Admin)
                                     || identityService.IsInRole(RoleType.Director);


    [RelayCommand]
    private void NavigateToJournal()
            => CurrentView = serviceProvider.GetRequiredService<JournalViewModel>();

    [RelayCommand]
    private void NavigateToAcademics()
        => CurrentView = serviceProvider.GetRequiredService<AcademicsViewModel>();

    [RelayCommand]
    private void NavigateToPeople()
        => CurrentView = serviceProvider.GetRequiredService<PeopleViewModel>();

    [RelayCommand]
    private void NavigateToSchedule()
            => CurrentView = serviceProvider.GetRequiredService<ScheduleViewModel>();

    [RelayCommand]
    private void NavigateToSettings()
        => CurrentView = serviceProvider.GetRequiredService<SettingsViewModel>();

    [RelayCommand]
    private void NavigateToAdministration()
        => CurrentView = serviceProvider.GetRequiredService<AdministrationViewModel>();

    [RelayCommand]
    private void NavigateToTesting()
        => CurrentView = serviceProvider.GetRequiredService<TestingViewModel>();

    [RelayCommand]
    private void NavigateToAnnouncements()
        => CurrentView = serviceProvider.GetRequiredService<AnnouncementsViewModel>();

    public event EventHandler<EventArgs>? OnLoggedOut;

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await authService.LogoutAsync(CancellationToken.None).ConfigureAwait(true);
        OnLoggedOut?.Invoke(this, EventArgs.Empty);
    }
}