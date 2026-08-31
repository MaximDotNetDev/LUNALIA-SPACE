using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Client.Core.Features.Identity.Common;
using SchoolJournal.Contracts.Enums.Identity;
using SchoolJournal.Contracts.DTOs.Identity.Login;
using SchoolJournal.Client.Core.Common.Interfaces;

namespace SchoolJournal.Client.Core.Features.Identity.Login;

public sealed partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IIdentityService _identityService;
    private readonly IUserThemeService _themeService;

    public LoginViewModel(IAuthService authService, IIdentityService identityService, IUserThemeService themeService)
    {
        _authService = authService;
        _identityService = identityService;
        _themeService = themeService;
        _ = TrySilentLoginAsync(CancellationToken.None);
    }

    public Action? OnLoginSuccess { get; set; }

    [ObservableProperty]
    public partial string Login { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [RelayCommand]
    private async Task ExecuteLoginAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Введіть логін та пароль.";
            return;
        }

        ErrorMessage = string.Empty;
        IsLoading = true;

        try
        {
            // Замінюємо Environment.MachineName на хардкод, оскільки WASM не має доступу до системи
            var deviceName = "Web Browser (Blazor)";

            var result = await _authService.LoginAsync(Login, Password, deviceName, ct).ConfigureAwait(true);

            if (result.IsError)
            {
                ErrorMessage = result.FirstError.Description;
            }
            else
            {
                ErrorMessage = string.Empty;

                _identityService.SetUser(result.Value.Role);
                await _themeService.SetUserAndLoadThemeAsync(Login).ConfigureAwait(true);

                OnLoginSuccess?.Invoke();
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Ловимо помилки мережі (наприклад, якщо API не запущено або проблема з CORS)
            ErrorMessage = $"Помилка з'єднання з сервером: {ex.Message}";
        }
        finally
        {
            // Гарантуємо, що лоадер зникне у БУДЬ-ЯКОМУ випадку (успіх, помилка пароля чи краш мережі)
            IsLoading = false;
        }
    }

    private async Task TrySilentLoginAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = "Відновлення сесії...";

        var result = await _authService.RefreshTokenAsync(ct).ConfigureAwait(true);

        if (!result.IsError)
        {
            ErrorMessage = string.Empty;
            IsLoading = false;

            // Якщо токен оновлено автоматично, використовуємо фолбек-користувача 
            // (або логін з IIdentityService, якщо він там згодом зберігатиметься)
            await _themeService.SetUserAndLoadThemeAsync("Default").ConfigureAwait(true);

            OnLoginSuccess?.Invoke();
            return;
        }

        ErrorMessage = string.Empty;
        IsLoading = false;
    }
}