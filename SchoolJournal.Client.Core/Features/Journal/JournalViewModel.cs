using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SchoolJournal.Client.Core.Features.Operations.Lessons;

namespace SchoolJournal.Client.Core.Features.Journal;

public sealed partial class JournalViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;

    public JournalViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        NavigateToLessons(); 
    }

    [ObservableProperty]
    public partial ObservableObject? CurrentJournalView { get; set; }

    [RelayCommand]
    private void NavigateToLessons()
        => CurrentJournalView = _serviceProvider.GetRequiredService<LessonsViewModel>();

    [RelayCommand]
    private void NavigateToGrades()
            => CurrentJournalView = _serviceProvider.GetRequiredService<SchoolJournal.Client.Core.Features.Operations.Grades.GradesViewModel>();

}