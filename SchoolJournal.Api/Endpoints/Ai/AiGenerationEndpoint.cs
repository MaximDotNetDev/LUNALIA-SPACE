using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.AiGeneration.GenerateFromPdf;
using SchoolJournal.Application.Features.AiGeneration.GenerateMultipleChoice;
using SchoolJournal.Contracts.DTOs.AiGeneration;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;
using SchoolJournal.Domain.Enums.Identity;
using SchoolJournal.Api.Endpoints.Ai.Requests;
using SchoolJournal.Application.Features.AiGeneration.GenerateTrueFalseFromPdf;
using SchoolJournal.Application.Features.AiGeneration.GenerateOddOneOutFromPdf;
using SchoolJournal.Application.Features.AiGeneration.GenerateGuessByDescriptionFromPdf;
using SchoolJournal.Application.Features.AiGeneration.GenerateProofreaderFromPdf;
using SchoolJournal.Application.Features.AiGeneration.GenerateAssociativeBushFromPdf;
using SchoolJournal.Application.Features.AiGeneration.GenerateCrosswordFromPdf;
using SchoolJournal.Application.Features.AiGeneration.GenerateFillwordFromPdf;
using SchoolJournal.Application.Features.AiGeneration.GenerateFillInTheBlankFromPdf;
using SchoolJournal.Application.Features.AiGeneration.GenerateMatchingFromPdf;

namespace SchoolJournal.Api.Endpoints.Ai;

internal static class AiGenerationEndpoint
{
    private const string AiTag = "AiGeneration";
    private const string FileNotUploadedMessage = "Файл не завантажено.";
    private const string PdfContentType = "application/pdf";
    private const string OnlyPdfSupportedMessage = "Підтримуються лише PDF файли.";

    private static IResult HandleError(ErrorOr.Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorOr.ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorOr.ErrorType.Failure => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(statusCode: statusCode, title: error.Description);
    }

    public static void MapAiGeneration(this IEndpointRouteBuilder app)
    {
        app.MapTextGenerationEndpoints();
        app.MapStandardPdfEndpoints();
        app.MapAdvancedPdfEndpoints();
    }

    private static void MapTextGenerationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/ai/generate-multiple-choice", async (
            [FromBody] GenerateMultipleChoiceRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new GenerateMultipleChoiceCommand(
                request.Text,
                request.TotalQuestions,
                request.MultiAnswerCount,
                request.JudgmentCount,
                request.PointsPerQuestion);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .WithAiMetadata("Генерація тесту з переданого тексту через Gemini AI (Admin, Director, Teacher)");
    }

    private static void MapStandardPdfEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/ai/generate-multiple-choice-from-pdf", (
            [AsParameters] GenerateMultipleChoiceFromPdfRequest req, ISender sender, CancellationToken ct) =>
            ProcessPdfRequestAsync(req.File, sender, bytes => new GenerateMultipleChoiceFromPdfCommand(
                bytes, req.StartPage, req.EndPage, req.TotalQuestions, req.MultiAnswerCount, req.JudgmentCount, req.PointsPerQuestion), ct))
        .WithAiMetadata("Генерація тесту з PDF файлу (Admin, Director, Teacher)", isFileUpload: true);

        app.MapPost("/api/ai/generate-true-false-from-pdf", (
            [AsParameters] GenerateTrueFalseFromPdfRequest req, ISender sender, CancellationToken ct) =>
            ProcessPdfRequestAsync(req.File, sender, bytes => new GenerateTrueFalseFromPdfCommand(
                bytes, req.StartPage, req.EndPage, req.QuestionCount, req.PointsPerQuestion), ct))
        .WithAiMetadata("Генерація тесту 'Так чи ні' з PDF файлу (Admin, Director, Teacher)", isFileUpload: true);

        app.MapPost("/api/ai/generate-odd-one-out-from-pdf", (
            [AsParameters] GenerateOddOneOutFromPdfRequest req, ISender sender, CancellationToken ct) =>
            ProcessPdfRequestAsync(req.File, sender, bytes => new GenerateOddOneOutFromPdfCommand(
                bytes, req.StartPage, req.EndPage, req.QuestionCount, req.PointsPerQuestion), ct))
        .WithAiMetadata("Генерація тесту 'Знайди зайве' з PDF (Admin, Director, Teacher)", isFileUpload: true);

        app.MapPost("/api/ai/generate-guess-by-description-from-pdf", (
            [AsParameters] GenerateGuessByDescriptionFromPdfRequest req, ISender sender, CancellationToken ct) =>
            ProcessPdfRequestAsync(req.File, sender, bytes => new GenerateGuessByDescriptionFromPdfCommand(
                bytes, req.StartPage, req.EndPage, req.QuestionCount, req.PointsPerQuestion), ct))
        .WithAiMetadata("Генерація тесту 'Відгадай за описом' з PDF (Admin, Director, Teacher)", isFileUpload: true);

        app.MapPost("/api/ai/generate-proofreader-from-pdf", (
            [AsParameters] GenerateProofreaderFromPdfRequest req, ISender sender, CancellationToken ct) =>
            ProcessPdfRequestAsync(req.File, sender, bytes => new GenerateProofreaderFromPdfCommand(
                bytes, req.StartPage, req.EndPage, req.QuestionCount, req.PointsPerQuestion), ct))
        .WithAiMetadata("Генерація завдання 'Коректор' з PDF (Admin, Director, Teacher)", isFileUpload: true);
    }

    private static void MapAdvancedPdfEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/ai/generate-crossword-from-pdf", (
            [AsParameters] GenerateCrosswordFromPdfRequest req, ISender sender, CancellationToken ct) =>
            ProcessPdfRequestAsync(req.File, sender, bytes => new GenerateCrosswordFromPdfCommand(
                bytes, req.StartPage, req.EndPage, req.WordCount, req.PointsPerWord), ct))
        .WithAiMetadata("Генерація 'Кросворду' з PDF матеріалу (Admin, Director, Teacher)", isFileUpload: true);

        app.MapPost("/api/ai/generate-associative-bush-from-pdf", (
            [AsParameters] GenerateAssociativeBushFromPdfRequest req, ISender sender, CancellationToken ct) =>
            ProcessPdfRequestAsync(req.File, sender, bytes => new GenerateAssociativeBushFromPdfCommand(
                bytes, req.StartPage, req.EndPage, req.QuestionCount, req.PointsPerQuestion), ct))
        .WithAiMetadata("Генерація завдання 'Асоціативний кущ' з PDF (Admin, Director, Teacher)", isFileUpload: true);

        app.MapPost("/api/ai/generate-fillword-from-pdf", (
            [AsParameters] GenerateFillwordFromPdfRequest req, ISender sender, CancellationToken ct) =>
            ProcessPdfRequestAsync(req.File, sender, bytes => new GenerateFillwordFromPdfCommand(
                bytes, req.StartPage, req.EndPage, req.WordCount, req.PointsPerWord), ct))
        .WithAiMetadata("Генерація 'Філворду' з PDF матеріалу (Admin, Director, Teacher)", isFileUpload: true);

        app.MapPost("/api/ai/generate-fill-blanks-from-pdf", (
            [AsParameters] GenerateFillInTheBlankFromPdfRequest req, ISender sender, CancellationToken ct) =>
            ProcessPdfRequestAsync(req.File, sender, bytes => new GenerateFillInTheBlankFromPdfCommand(
                bytes, req.StartPage, req.EndPage, req.QuestionCount, req.PointsPerQuestion), ct))
        .WithAiMetadata("Генерація тесту 'Вставте пропущені слова' з PDF (Admin, Director, Teacher)", isFileUpload: true);

        app.MapPost("/api/ai/generate-matching-from-pdf", (
            [AsParameters] GenerateMatchingFromPdfRequest req, ISender sender, CancellationToken ct) =>
            ProcessPdfRequestAsync(req.File, sender, bytes => new GenerateMatchingFromPdfCommand(
                bytes, req.StartPage, req.EndPage, req.QuestionCount, req.PointsPerQuestion), ct))
        .WithAiMetadata("Генерація тесту 'Встановіть відповідність' з PDF (Admin, Director, Teacher)", isFileUpload: true);
    }

    private static async Task<IResult> ProcessPdfRequestAsync<TCommand>(
        IFormFile? file,
        ISender sender,
        Func<IReadOnlyCollection<byte>, TCommand> commandFactory,
        CancellationToken cancellationToken) where TCommand : MediatR.IRequest<ErrorOr.ErrorOr<GeneratedQuizResponse>>
    {
        if (file is null || file.Length == 0)
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: FileNotUploadedMessage);

        if (file.ContentType != PdfContentType)
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: OnlyPdfSupportedMessage);

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);

        var command = commandFactory(memoryStream.ToArray());
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

        return result.IsError ? HandleError(result.FirstError) : Results.Ok(result.Value);
    }

    private static void WithAiMetadata(this RouteHandlerBuilder builder, string summary, bool isFileUpload = false)
    {
        if (isFileUpload)
        {
            builder.DisableAntiforgery();
        }

        builder
            .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
            .WithTags(AiTag)
            .WithSummary(summary)
            .Produces<GeneratedQuizResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);
    }
}