using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Operations.Quizzes.CreateQuiz;
using SchoolJournal.Application.Features.Operations.Quizzes.DeleteQuiz;
using SchoolJournal.Application.Features.Operations.Quizzes.GetQuizById;
using SchoolJournal.Application.Features.Operations.Quizzes.GetQuizzesBySubject;
using SchoolJournal.Application.Features.Operations.Quizzes.GetQuizzesByTeacher;
using SchoolJournal.Application.Features.Operations.Quizzes.GetQuizzesPaged;
using SchoolJournal.Application.Features.Operations.Quizzes.UpdateQuiz;
using SchoolJournal.Application.Features.Operations.Quizzes.SaveGeneratedQuiz;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Operations;

internal static class QuizzesEndpoint
{
    private const string OperationsTag = "Operations";

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

    public static void MapQuizzes(this IEndpointRouteBuilder app)
    {
        app.MapQuizCommands();
        app.MapQuizQueries();
    }

    private static void MapQuizCommands(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/quizzes", async (
            [FromBody] CreateQuizRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateQuizCommand(
                request.TeacherId,
                request.SubjectId,
                request.Title);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(new { QuizId = result.Value });
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
        .WithTags(OperationsTag)
        .WithSummary("Створення нового тесту (Admin, Director, Teacher)")
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPost("/api/quizzes/save-generated", async (
                    [FromBody] SaveGeneratedQuizRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
        {
            var command = new SaveGeneratedQuizCommand(
                request.TeacherId,
                request.SubjectId,
                request.ClassId,
                request.Title,
                [.. request.Questions.Select(q => new SaveGeneratedQuizQuestionCommandItem(
                    q.OrderIndex,
                    q.QuestionText,
                    q.QuestionType,
                    q.ContentJson,
                    q.Points))]);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(new { QuizId = result.Value });
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
        .WithTags(OperationsTag)
        .WithSummary("Збереження згенерованого ШІ тесту разом з питаннями в бібліотеку (Admin, Director, Teacher)")
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPut("/api/quizzes/{id:guid}", async (
                    [FromRoute] Guid id,
                    [FromBody] UpdateQuizRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
        {
            var command = new UpdateQuizCommand(
                id,
                request.SubjectId,
                request.Title,
                request.RowVersionBase64,
                [.. request.Questions.Select(q => new UpdateQuizQuestionCommand(
                    q.QuestionId,
                    q.OrderIndex,
                    q.QuestionText,
                    q.QuestionType,
                    q.ContentJson,
                    q.Points))]);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
        .WithTags(OperationsTag)
        .WithSummary("Оновлення існуючого тесту (Admin, Director, Teacher)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapDelete("/api/quizzes/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] DeleteQuizRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteQuizCommand(id, request.RowVersionBase64);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
        .WithTags(OperationsTag)
        .WithSummary("М'яке видалення тесту (Admin, Director, Teacher)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static void MapQuizQueries(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/quizzes/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetQuizByIdQuery(id);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
        .WithTags(OperationsTag)
        .WithSummary("Отримання деталей тесту за ID (Admin, Director, Teacher)")
        .Produces<QuizDetailResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapGet("/api/teachers/{teacherId:guid}/quizzes", async (
            [FromRoute] Guid teacherId,
            [AsParameters] TeacherQuizzesRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetQuizzesByTeacherQuery(
                teacherId,
                new PageRequest(request.PageNumber, request.PageSize));

            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
        .WithTags(OperationsTag)
        .WithSummary("Отримання списку тестів конкретного викладача (Admin, Director, Teacher)")
        .Produces<PagedResponse<QuizResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapGet("/api/subjects/{subjectId:guid}/quizzes", async (
            [FromRoute] Guid subjectId,
            [AsParameters] SubjectQuizzesRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetQuizzesBySubjectQuery(
                subjectId,
                new PageRequest(request.PageNumber, request.PageSize));

            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
        .WithTags(OperationsTag)
        .WithSummary("Отримання списку тестів за предметом (Admin, Director, Teacher)")
        .Produces<PagedResponse<QuizResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapGet("/api/quizzes", async (
            [AsParameters] QuizzesSearchRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetQuizzesPagedQuery(
                request.SearchTerm,
                new PageRequest(request.PageNumber, request.PageSize));

            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
        .WithTags(OperationsTag)
        .WithSummary("Глобальний пошук та пагінація тестів (Admin, Director, Teacher)")
        .Produces<PagedResponse<QuizResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}