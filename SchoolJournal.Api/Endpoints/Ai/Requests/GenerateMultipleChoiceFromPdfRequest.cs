using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;


namespace SchoolJournal.Api.Endpoints.Ai.Requests;

[SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "DTO for Minimal APIs needs to be public for Model Binding and Swagger.")]
public sealed class GenerateMultipleChoiceFromPdfRequest
{
    public IFormFile? File { get; init; }
    public int TotalQuestions { get; init; } = 12;
    public int MultiAnswerCount { get; init; }
    public int JudgmentCount { get; init; }
    public int PointsPerQuestion { get; init; } = 1;
    public int? StartPage { get; init; }
    public int? EndPage { get; init; }
}