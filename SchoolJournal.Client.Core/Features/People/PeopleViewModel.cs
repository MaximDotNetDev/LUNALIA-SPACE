using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SchoolJournal.Client.Core.Features.Core.Parents;
using SchoolJournal.Client.Core.Features.Core.Students;
using SchoolJournal.Client.Core.Features.Core.Teachers;
using SchoolJournal.Client.Core.Features.Core.StudentParents;

namespace SchoolJournal.Client.Core.Features.People;

public sealed partial class PeopleViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;

    public PeopleViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        NavigateToTeachers();
    }

    [ObservableProperty]
    public partial ObservableObject? CurrentPeopleView { get; set; }

    [RelayCommand]
    private void NavigateToTeachers()
        => CurrentPeopleView = _serviceProvider.GetRequiredService<TeachersViewModel>();

    [RelayCommand]
    private void NavigateToParents()
        => CurrentPeopleView = _serviceProvider.GetRequiredService<ParentsViewModel>();

    [RelayCommand]
    private void NavigateToStudents()
        => CurrentPeopleView = _serviceProvider.GetRequiredService<StudentsViewModel>();

    [RelayCommand]
    private void NavigateToStudentParents()
        => CurrentPeopleView = _serviceProvider.GetRequiredService<StudentParentsViewModel>();
}