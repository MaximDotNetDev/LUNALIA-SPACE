using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SchoolJournal.Desktop.Common.Converters;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1515:Consider making public types internal")]
public sealed class UniversalVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isVisible = value switch
        {
            null => false,
            bool b => b,
            int i => i > 0,
            string s => !string.IsNullOrWhiteSpace(s),
            IEnumerable en => en.GetEnumerator().MoveNext(),
            _ => true
        };

        if (parameter?.ToString() == "Inverse") isVisible = !isVisible;

        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}