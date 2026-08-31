using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

namespace SchoolJournal.Api.Endpoints.Ai.Requests;

[SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "DTO for Minimal APIs needs to be public for Model Binding and Swagger.")]
public sealed class GenerateGuessByDescriptionFromPdfRequest
{
    public IFormFile? File { get; init; }
    public int QuestionCount { get; init; } = 5;
    public int PointsPerQuestion { get; init; } = 1;
    public int? StartPage { get; init; }
    public int? EndPage { get; init; }
}