using Microsoft.Extensions.DependencyInjection;
using SchoolJournal.Client.Core.Features.Identity.Common;
using SchoolJournal.Client.Core.Features.Identity.Login;
using SchoolJournal.Client.Core.Features.Shell;
using SchoolJournal.Desktop.Features.Identity.Login;
using SchoolJournal.Desktop.Features.Infrastructure.Logs;
using System.Windows;

namespace SchoolJournal.Desktop.Features.Shell;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1515:Consider making public types internal")]
public sealed partial class MainWindow : Window
{
    private readonly IServiceProvider _serviceProvider;

    public MainWindow(IServiceProvider serviceProvider, MainViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(viewModel);

        this.InitializeComponent();
        this._serviceProvider = serviceProvider;
        this.DataContext = viewModel;

        viewModel.OnLoggedOut += ViewModel_OnLoggedOut;
    }

    private void ViewModel_OnLoggedOut(object? sender, EventArgs e)
    {
        if (this.DataContext is MainViewModel viewModel)
        {
            viewModel.OnLoggedOut -= ViewModel_OnLoggedOut;
        }

        var loginWindow = this._serviceProvider.GetRequiredService<LoginWindow>();

        Application.Current.MainWindow = loginWindow;
        loginWindow.Show();

        this.Close();
    }
}