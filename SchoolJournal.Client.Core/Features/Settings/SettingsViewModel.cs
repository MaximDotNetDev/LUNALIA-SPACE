using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Client.Core.Features.Reference.Semester;
using SchoolJournal.Contracts.Enums.Identity;
using SchoolJournal.Client.Core.Features.Infrastructure.SystemSettings;
using SchoolJournal.Client.Core.Features.Reference.Position;
using SchoolJournal.Client.Core.Features.Reference.Qualification;
using SchoolJournal.Client.Core.Features.Reference.PedagogicalTitle;
using SchoolJournal.Client.Core.Features.Reference.GradeType;
using SchoolJournal.Client.Core.Features.Reference.LessonType;
using SchoolJournal.Client.Core.Features.Reference.BellSchedule;
using SchoolJournal.Client.Core.Features.Reference.Classroom;
using SchoolJournal.Client.Core.Features.Core.Subject;

namespace SchoolJournal.Client.Core.Features.Settings;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IIdentityService _identityService;

    public SettingsViewModel(IServiceProvider serviceProvider, IIdentityService identityService)
    {
        _serviceProvider = serviceProvider;
        _identityService = identityService;

        NavigateToSemesters();
    }

    [ObservableProperty]
    private ObservableObject? _currentSettingsView;

    public bool IsAdmin => _identityService.IsInRole(RoleType.Admin);

    [RelayCommand]
    private void NavigateToSemesters()
        => CurrentSettingsView = _serviceProvider.GetRequiredService<SemestersViewModel>();

    [RelayCommand]
    private void NavigateToSystemSettings()
        => CurrentSettingsView = _serviceProvider.GetRequiredService<SystemSettingsViewModel>();

    [RelayCommand]
    private void NavigateToPositions()
        => CurrentSettingsView = _serviceProvider.GetRequiredService<PositionsViewModel>();

    [RelayCommand]
    private void NavigateToQualifications()
        => CurrentSettingsView = _serviceProvider.GetRequiredService<QualificationsViewModel>();

    [RelayCommand]
    private void NavigateToPedagogicalTitles()
        => CurrentSettingsView = _serviceProvider.GetRequiredService<PedagogicalTitlesViewModel>();

    [RelayCommand]
    private void NavigateToGradeTypes()
        => CurrentSettingsView = _serviceProvider.GetRequiredService<GradeTypesViewModel>();

    [RelayCommand]
    private void NavigateToLessonTypes()
        => CurrentSettingsView = _serviceProvider.GetRequiredService<LessonTypesViewModel>();

    [RelayCommand]
    private void NavigateToBellSchedules()
        => CurrentSettingsView = _serviceProvider.GetRequiredService<BellSchedulesViewModel>();

    [RelayCommand]
    private void NavigateToClassrooms()
        => CurrentSettingsView = _serviceProvider.GetRequiredService<ClassroomsViewModel>();

    [RelayCommand]
    private void NavigateToSubjects()
        => CurrentSettingsView = _serviceProvider.GetRequiredService<SubjectsViewModel>();
}