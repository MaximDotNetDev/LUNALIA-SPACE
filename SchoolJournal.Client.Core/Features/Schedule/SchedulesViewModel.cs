using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SchoolJournal.Client.Core.Features.Operations.FixedSchedules;

namespace SchoolJournal.Client.Core.Features.Schedule;

public sealed partial class ScheduleViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;

    public ScheduleViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        NavigateToFixedSchedules();
    }

    [ObservableProperty]
    public partial ObservableObject? CurrentScheduleView { get; set; }

    [RelayCommand]
    private void NavigateToFixedSchedules()
        => CurrentScheduleView = _serviceProvider.GetRequiredService<FixedSchedulesViewModel>();
}