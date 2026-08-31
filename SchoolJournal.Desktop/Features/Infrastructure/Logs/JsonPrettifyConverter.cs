using System.Globalization;
using System.Text.Json;
using System.Windows.Data;

namespace SchoolJournal.Desktop.Features.Infrastructure.Logs;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1515:Consider making public types internal")]
public sealed class JsonPrettifyConverter : IValueConverter
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string json || string.IsNullOrWhiteSpace(json))
        {
            return value;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(document.RootElement, _jsonOptions);
        }
        catch (JsonException)
        {
            return value;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}