using System.Text.Json.Serialization;

namespace SchoolJournal.Infrastructure.Services.Ai.Models;

public sealed class GeminiPart
{
    [JsonPropertyName("text")]
    public string? Text { get; init; }
}

public sealed class GeminiContent
{
    [JsonPropertyName("parts")]
    public IReadOnlyCollection<GeminiPart>? Parts { get; init; }
}

public sealed class GeminiCandidate
{
    [JsonPropertyName("content")]
    public GeminiContent? Content { get; init; }
}

public sealed class GeminiResponse
{
    [JsonPropertyName("candidates")]
    public IReadOnlyCollection<GeminiCandidate>? Candidates { get; init; }
}