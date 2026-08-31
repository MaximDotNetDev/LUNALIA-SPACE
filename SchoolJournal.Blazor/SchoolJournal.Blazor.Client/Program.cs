using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SchoolJournal.Blazor.Client.Services;
using SchoolJournal.Client.Core;
using SchoolJournal.Client.Core.Common.Auth;
using SchoolJournal.Client.Core.Common.Interfaces;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

string? apiUrl = builder.Configuration["ApiBaseUrl"];
if (string.IsNullOrWhiteSpace(apiUrl))
{
    throw new InvalidOperationException("Налаштування 'ApiBaseUrl' відсутнє у файлі wwwroot/appsettings.json.");
}

// 1. Реєструємо клієнтське ядро з URL (як у WPF)
builder.Services.AddClientCore(new Uri(apiUrl));

// 2. Реєструємо специфічне для Web сховище токенів
builder.Services.AddSingleton<ITokenStorageService, BrowserTokenStorageService>();

// 3. Реєструємо сервіс управління темами для Web
builder.Services.AddSingleton<IUserThemeService, BrowserThemeService>();

// 4. Реєструємо ViewModels (Transient, щоб при кожному заході на сторінку створювалась нова модель)
builder.Services.AddTransient<SchoolJournal.Client.Core.Features.Identity.Login.LoginViewModel>();

// 5. Реєструємо головну модель оболонки (Scoped, щоб стан меню зберігався під час сесії)
builder.Services.AddScoped<SchoolJournal.Client.Core.Features.Shell.MainViewModel>();

// 6. Реєструємо ViewModels функціональних модулів (Transient)
builder.Services.AddTransient<SchoolJournal.Client.Core.Features.Testing.TestingViewModel>();

await builder.Build().RunAsync().ConfigureAwait(false);