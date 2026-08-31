using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SchoolJournal.Client.Core.Features.Core.SchoolClasses;
using SchoolJournal.Client.Core.Features.Core.Subgroups;
using SchoolJournal.Client.Core.Features.Core.StudentSubgroups;
using SchoolJournal.Client.Core.Features.Operations.TeachingAssignments;
using SchoolJournal.Client.Core.Features.Operations.TeacherSubstitution;

namespace SchoolJournal.Client.Core.Features.Academics;

public sealed partial class AcademicsViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;

    public AcademicsViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        NavigateToSchoolClasses();
    }

    [ObservableProperty]
    public partial ObservableObject? CurrentAcademicsView { get; set; }

    [RelayCommand]
    private void NavigateToSchoolClasses()
        => CurrentAcademicsView = _serviceProvider.GetRequiredService<SchoolClassesViewModel>();

    [RelayCommand]
    private void NavigateToSubgroups()
        => CurrentAcademicsView = _serviceProvider.GetRequiredService<SubgroupsViewModel>();

    [RelayCommand]
    private void NavigateToStudentSubgroups()
        => CurrentAcademicsView = _serviceProvider.GetRequiredService<StudentSubgroupsViewModel>();

    [RelayCommand]
    private void NavigateToTeachingAssignments()
        => CurrentAcademicsView = _serviceProvider.GetRequiredService<TeachingAssignmentsViewModel>();

    [RelayCommand]
    private void NavigateToTeacherSubstitutions()
        => CurrentAcademicsView = _serviceProvider.GetRequiredService<TeacherSubstitutionsViewModel>();

}