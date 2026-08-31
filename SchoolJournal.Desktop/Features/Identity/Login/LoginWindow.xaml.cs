using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SchoolJournal.Client.Core.Features.Identity.Login;
using SchoolJournal.Desktop.Features.Shell;

namespace SchoolJournal.Desktop.Features.Identity.Login;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1515:Consider making public types internal")]
public sealed partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;

    public LoginWindow(LoginViewModel viewModel, IServiceProvider serviceProvider)
    {
        this.InitializeComponent();
        this._viewModel = viewModel;
        this._serviceProvider = serviceProvider;
        this.DataContext = this._viewModel;

        this._viewModel.OnLoginSuccess += this.ViewModel_OnLoginSuccess;
    }

    private void ViewModel_OnLoginSuccess()
    {
        if (System.Windows.Interop.ComponentDispatcher.IsThreadModal)
        {
            this.DialogResult = true;
            this.Close();
        }
        else
        {
            var mainWindow = this._serviceProvider.GetRequiredService<MainWindow>();
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
            this.Close();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        this._viewModel.OnLoginSuccess -= this.ViewModel_OnLoginSuccess;
        base.OnClosed(e);
    }

    private void UserPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        this._viewModel.Password = this.UserPasswordBox.Password;
    }

    private void ThemeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (sender is System.Windows.Controls.ComboBox comboBox &&
            comboBox.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
        {
            var themeName = selectedItem.Content.ToString();
            if (!string.IsNullOrWhiteSpace(themeName))
            {
                SchoolJournal.Desktop.Common.Services.ThemeManager.ApplyTheme(themeName);
            }
        }
    }
}