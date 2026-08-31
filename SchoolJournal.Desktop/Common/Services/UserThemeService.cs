using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SchoolJournal.Client.Core.Common.Interfaces;

namespace SchoolJournal.Desktop.Common.Services;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via Dependency Injection")]
internal sealed class UserThemeService : IUserThemeService
{
    private readonly string _settingsFilePath;
    private string _currentUser = "Default";
    private string _currentTheme = "Light";

    public string CurrentTheme => _currentTheme;

    public UserThemeService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var directory = Path.Combine(appData, "Lunalia");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        _settingsFilePath = Path.Combine(directory, "users_themes.json");
    }

    public async Task SetUserAndLoadThemeAsync(string username)
    {
        _currentUser = string.IsNullOrWhiteSpace(username) ? "Default" : username;

        var themes = await LoadSettingsAsync().ConfigureAwait(false);
        if (themes.TryGetValue(_currentUser, out var savedTheme))
        {
            _currentTheme = savedTheme;
        }
        else
        {
            _currentTheme = "Cosmic";
        }

        ThemeManager.ApplyTheme(_currentTheme);
    }

    public async Task ApplyAndSaveThemeAsync(string themeName)
    {
        if (string.IsNullOrWhiteSpace(themeName)) return;

        _currentTheme = themeName;
        ThemeManager.ApplyTheme(themeName);

        var themes = await LoadSettingsAsync().ConfigureAwait(false);
        themes[_currentUser] = themeName;
        await SaveSettingsAsync(themes).ConfigureAwait(false);
    }

    private async Task<Dictionary<string, string>> LoadSettingsAsync()
    {
        if (!File.Exists(_settingsFilePath)) return [];

        try
        {
            using var stream = new FileStream(_settingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            return await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream).ConfigureAwait(false) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
        catch (IOException)
        {
            return [];
        }
    }

    private async Task SaveSettingsAsync(Dictionary<string, string> settings)
    {
        try
        {
            using var stream = new FileStream(_settingsFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
            await JsonSerializer.SerializeAsync(stream, settings).ConfigureAwait(false);
        }
        catch (JsonException)
        {
            // Ignore format errors
        }
        catch (IOException)
        {
            // Ignore file access errors
        }
    }
}