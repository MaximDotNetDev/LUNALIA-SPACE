using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

namespace SchoolJournal.Api.Endpoints.Ai.Requests;

[SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "DTO for Minimal APIs needs to be public for Model Binding and Swagger.")]
public sealed class GenerateFillwordFromPdfRequest
{
    public IFormFile? File { get; init; }
    public int WordCount { get; init; } = 10;
    public int PointsPerWord { get; init; } = 1;
    public int? StartPage { get; init; }
    public int? EndPage { get; init; }
}