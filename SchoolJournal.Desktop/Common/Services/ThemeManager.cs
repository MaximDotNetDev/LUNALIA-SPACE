using System;
using System.Linq;
using System.Windows;

namespace SchoolJournal.Desktop.Common.Services;

internal static class ThemeManager
{
    // Використано спрощену ініціалізацію колекції (C# 12+)
    private static readonly string[] PaletteFiles =
    [
        "LightTheme.xaml",
        "CosmicPalette.xaml",
        "CalmEyesPalette.xaml",
        "GreenTheme.xaml"
    ];

    public static void ApplyTheme(string themeName)
    {
        var fileName = themeName switch
        {
            "Cosmic" => "CosmicPalette.xaml",
            "Calm" => "CalmEyesPalette.xaml",
            "Green" => "GreenTheme.xaml",
            _ => "LightTheme.xaml"
        };

        var uri = new Uri($"Resources/Styles/{fileName}", UriKind.Relative);
        var appResources = Application.Current.Resources.MergedDictionaries;

        // Пошук існуючої палітри з використанням StringComparison для чіткості намірів
        var oldPalette = appResources.FirstOrDefault(d =>
            d.Source != null &&
            PaletteFiles.Any(p => d.Source.ToString().Contains(p, StringComparison.OrdinalIgnoreCase)));

        if (oldPalette != null)
        {
            appResources.Remove(oldPalette);
        }

        appResources.Add(new ResourceDictionary { Source = uri });
    }
}