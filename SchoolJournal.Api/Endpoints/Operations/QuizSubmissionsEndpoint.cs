using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Operations.QuizSubmissions.GetAssignmentSubmissions;

// УВАГА: Якщо твоя команда називається інакше або лежить в іншій папці, підключи правильний namespace
using SchoolJournal.Application.Features.Operations.QuizSubmissions.SubmitQuiz;
using SchoolJournal.Contracts.DTOs.Operations.QuizSubmissions;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Operations;

internal static class QuizSubmissionsEndpoint
{
    private const string OperationsTag = "QuizSubmissions";

    private static IResult HandleError(ErrorOr.Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorOr.ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorOr.ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorOr.ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorOr.ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(statusCode: statusCode, title: error.Description);
    }

    public static void MapQuizSubmissions(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/quiz-submissions", async (
            [FromBody] SubmitQuizRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            // Формуємо команду (Command) для передачі в Application шар (CQRS)
            var command = new SubmitQuizCommand(request.AssignmentId, request.Answers);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher, RoleType.Student) // КРИТИЧНО: Пускаємо учнів (Student)
        .WithTags(OperationsTag)
        .WithSummary("Здача тесту на перевірку (Admin, Director, Teacher, Student)")
        .Produces<SubmitQuizResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // НОВИЙ ЕНДПОІНТ: Отримання результатів
        app.MapGet("/api/quiz-submissions/assignment/{assignmentId:guid}", async (
            Guid assignmentId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new SchoolJournal.Application.Features.Operations.QuizSubmissions.GetAssignmentSubmissions.GetAssignmentSubmissionsQuery(assignmentId);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
        .WithTags(OperationsTag)
        .WithSummary("Отримання результатів тестування для конкретного призначення (Admin, Director, Teacher)")
        .Produces<List<QuizSubmissionResultDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden);

    }
}