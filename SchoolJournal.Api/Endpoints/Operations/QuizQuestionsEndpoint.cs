using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Operations.QuizQuestions.CreateQuizQuestion;
using SchoolJournal.Application.Features.Operations.QuizQuestions.DeleteQuizQuestion;
using SchoolJournal.Application.Features.Operations.QuizQuestions.GetQuizQuestionById;
using SchoolJournal.Application.Features.Operations.QuizQuestions.ReorderQuizQuestions;
using SchoolJournal.Application.Features.Operations.QuizQuestions.UpdateQuizQuestion;
using SchoolJournal.Application.Features.Operations.QuizQuestions.GetQuizQuestionsByQuizId;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Operations.QuizQuestions;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Operations;

internal static class QuizQuestionsEndpoint
{
    private const string OperationsTag = "Operations";

    private static IResult HandleError(ErrorOr.Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorOr.ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorOr.ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorOr.ErrorType.Validation => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(statusCode: statusCode, title: error.Description);
    }

    public static void MapQuizQuestions(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/quizzes/{quizId:guid}/questions", async (
            [FromRoute] Guid quizId,
            [FromBody] CreateQuizQuestionRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateQuizQuestionCommand(
                quizId,
                request.QuestionText,
                request.QuestionType,
                request.ContentJson,
                request.Points);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(new { QuestionId = result.Value });
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
        .WithTags(OperationsTag)
        .WithSummary("Додавання нового питання до тесту (Admin, Director, Teacher)")
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPut("/api/quiz-questions/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateQuizQuestionRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateQuizQuestionCommand(
                id,
                request.QuestionText,
                request.QuestionType,
                request.ContentJson,
                request.Points,
                request.RowVersionBase64);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
        .WithTags(OperationsTag)
        .WithSummary("Оновлення питання тесту з перевіркою версій (Admin, Director, Teacher)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapDelete("/api/quiz-questions/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] DeleteQuizQuestionRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteQuizQuestionCommand(
                id,
                request.RowVersionBase64);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
        .WithTags(OperationsTag)
        .WithSummary("М'яке видалення питання тесту (Admin, Director, Teacher)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPost("/api/quizzes/{quizId:guid}/questions/reorder", async (
            [FromRoute] Guid quizId,
            [FromBody] ReorderQuizQuestionsRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new ReorderQuizQuestionsCommand(
                quizId,
                request.Items);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
        .WithTags(OperationsTag)
        .WithSummary("Зміна порядку питань у тесті (Admin, Director, Teacher)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapGet("/api/quiz-questions/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetQuizQuestionByIdQuery(id);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
        .WithTags(OperationsTag)
        .WithSummary("Отримання деталей питання за ID (Admin, Director, Teacher)")
        .Produces<QuizQuestionResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapGet("/api/quizzes/{quizId:guid}/questions", async (
            [FromRoute] Guid quizId,
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetQuizQuestionsByQuizIdQuery(
                quizId,
                new PageRequest(pageNumber, pageSize));

            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher, RoleType.Student)
        .WithTags(OperationsTag)
        .WithSummary("Отримання списку питань для тесту з пагінацією (Admin, Director, Teacher, Student)")
        .Produces<PagedResponse<QuizQuestionResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}