using Microsoft.JSInterop;
using SchoolJournal.Client.Core.Common.Interfaces;
using System.Text.Json;

namespace SchoolJournal.Blazor.Client.Services;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by DI container")]
internal sealed class BrowserThemeService(IJSRuntime jsRuntime) : IUserThemeService
{
    private const string ThemesKey = "user_themes";
    private string _currentUser = "Default";
    private string _currentTheme = "Light";

    public string CurrentTheme => _currentTheme;

    public async Task SetUserAndLoadThemeAsync(string username)
    {
        _currentUser = string.IsNullOrWhiteSpace(username) ? "Default" : username;

        var themes = await LoadSettingsAsync().ConfigureAwait(false);
        _currentTheme = themes.TryGetValue(_currentUser, out var savedTheme) ? savedTheme : "Light";

        await ApplyThemeToBrowserAsync(_currentTheme).ConfigureAwait(false);
    }

    public async Task ApplyAndSaveThemeAsync(string themeName)
    {
        if (string.IsNullOrWhiteSpace(themeName)) return;

        _currentTheme = themeName;
        await ApplyThemeToBrowserAsync(themeName).ConfigureAwait(false);

        var themes = await LoadSettingsAsync().ConfigureAwait(false);
        themes[_currentUser] = themeName;
        await SaveSettingsAsync(themes).ConfigureAwait(false);
    }

    private async Task<Dictionary<string, string>> LoadSettingsAsync()
    {
        try
        {
            var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", ThemesKey).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(json)) return [];

            return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private async Task SaveSettingsAsync(Dictionary<string, string> settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings);
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", ThemesKey, json).ConfigureAwait(false);
        }
        catch (JsonException)
        {
            // Ігноруємо помилки серіалізації
        }
    }

    private async Task ApplyThemeToBrowserAsync(string themeName)
    {
        // Встановлюємо атрибут data-theme на тегу <body> через JS
        await jsRuntime.InvokeVoidAsync("document.body.setAttribute", "data-theme", themeName).ConfigureAwait(false);
    }
}