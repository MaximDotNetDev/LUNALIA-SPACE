using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Operations.QuizAssignments.CreateQuizAssignment;
using SchoolJournal.Application.Features.Operations.QuizAssignments.DeleteQuizAssignment;
using SchoolJournal.Application.Features.Operations.QuizAssignments.UpdateQuizAssignmentDueDate;
using SchoolJournal.Application.Features.Operations.QuizAssignments.GetQuizAssignmentById;
using SchoolJournal.Application.Features.Operations.QuizAssignments.GetActiveQuizAssignmentsByClassId;
using SchoolJournal.Application.Features.Operations.QuizAssignments.GetQuizAssignmentsByQuizId;
using SchoolJournal.Contracts.DTOs.Operations.QuizAssignments;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Operations;

internal static class QuizAssignmentsEndpoint
{
    private const string OperationsTag = "QuizAssignments";

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

    public static void MapQuizAssignments(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/quiz-assignments", async (
            [FromBody] CreateQuizAssignmentRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateQuizAssignmentCommand(
                request.QuizId,
                request.ClassId,
                request.DueDate);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(new { AssignmentId = result.Value });
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
        .WithTags(OperationsTag)
        .WithSummary("Призначити тест класу (Admin, Director, Teacher)")
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPut("/api/quiz-assignments/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateQuizAssignmentDueDateRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateQuizAssignmentDueDateCommand(
                id,
                request.DueDate,
                request.RowVersionBase64);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
        .WithTags(OperationsTag)
        .WithSummary("Оновлення терміну здачі призначення тесту (Admin, Director, Teacher)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapDelete("/api/quiz-assignments/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] DeleteQuizAssignmentRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteQuizAssignmentCommand(
                id,
                request.RowVersionBase64);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
        .WithTags(OperationsTag)
        .WithSummary("Скасування (видалення) призначення тесту (Admin, Director, Teacher)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapGet("/api/quiz-assignments/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetQuizAssignmentByIdQuery(id);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
        .WithTags(OperationsTag)
        .WithSummary("Отримання деталей призначення тесту за ID (Admin, Director, Teacher)")
        .Produces<QuizAssignmentResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapGet("/api/quiz-assignments/class/{classId:guid}", async (
            [FromRoute] Guid classId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetActiveQuizAssignmentsByClassIdQuery(classId);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher, RoleType.Student)
        .WithTags(OperationsTag)
        .WithSummary("Отримання активних призначень тестів для конкретного класу (Admin, Director, Teacher)")
        .Produces<IReadOnlyCollection<QuizAssignmentResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden);

        app.MapGet("/api/quiz-assignments/quiz/{quizId:guid}", async (
            [FromRoute] Guid quizId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetQuizAssignmentsByQuizIdQuery(quizId);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
        .WithTags(OperationsTag)
        .WithSummary("Отримання активних призначень для конкретного тесту (Admin, Director, Teacher)")
        .Produces<IReadOnlyCollection<QuizAssignmentResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

    }
}