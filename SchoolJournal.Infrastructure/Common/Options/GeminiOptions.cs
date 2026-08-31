using System.ComponentModel.DataAnnotations;

namespace SchoolJournal.Infrastructure.Common.Options;

public sealed class GeminiOptions
{
    public const string SectionName = "Gemini";

    [Required(AllowEmptyStrings = false, ErrorMessage = "ApiKey для Gemini є обов'язаковим.")]
    public string ApiKey { get; init; } = string.Empty;

    [Required(ErrorMessage = "BaseUrl для Gemini є обов'язаковим.")]
    public Uri BaseUrl { get; init; } = default!;
}