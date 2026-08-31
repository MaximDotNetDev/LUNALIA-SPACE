using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SchoolJournal.Client.Core.Features.Infrastructure.Logs;
using SchoolJournal.Client.Core.Features.Infrastructure.Outbox;

namespace SchoolJournal.Client.Core.Features.Administration;

public sealed partial class AdministrationViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;

    public AdministrationViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        NavigateToAuditLogs(); 
    }

    [ObservableProperty]
    public partial ObservableObject? CurrentAdminView { get; set; }

    [RelayCommand]
    private void NavigateToAuditLogs()
        => CurrentAdminView = _serviceProvider.GetRequiredService<AuditLogsViewModel>();

    [RelayCommand]
    private void NavigateToOutbox()
        => CurrentAdminView = _serviceProvider.GetRequiredService<OutboxViewModel>();
}