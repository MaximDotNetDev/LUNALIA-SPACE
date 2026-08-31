using System.Threading.Tasks;

namespace SchoolJournal.Client.Core.Common.Interfaces;

public interface IUserThemeService
{
    public Task SetUserAndLoadThemeAsync(string username);
    public Task ApplyAndSaveThemeAsync(string themeName);
    public string CurrentTheme { get; }
}