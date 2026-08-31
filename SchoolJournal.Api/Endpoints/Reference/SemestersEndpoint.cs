using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Reference.Semester.CreateSemester;
using SchoolJournal.Application.Features.Reference.Semester.DeleteSemester;
using SchoolJournal.Application.Features.Reference.Semester.RestoreSemester;
using SchoolJournal.Application.Features.Reference.Semester.UpdateSemester;
using SchoolJournal.Application.Features.Reference.Semester.GetActiveSemesters;
using SchoolJournal.Application.Features.Reference.Semester.GetDeletedSemesters;
using SchoolJournal.Application.Features.Reference.Semester.GetSemesterById;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.Semesters;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Reference;

internal static class SemestersEndpoint
{
    private const string ReferenceTag = "Reference";

    private static IResult HandleError(ErrorOr.Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorOr.ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorOr.ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorOr.ErrorType.Validation => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(statusCode: statusCode, title: error.Description);
    }

    public static void MapSemesters(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/semesters", async (
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetActiveSemestersQuery(
                new PageRequest(pageNumber, pageSize));

            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithTags(ReferenceTag)
        .WithSummary("Отримання списку активних семестрів (Всі ролі)")
        .Produces<PagedResponse<SemesterResponse>>(StatusCodes.Status200OK);

        app.MapGet("/api/semesters/archive", async (
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetDeletedSemestersQuery(
                new PageRequest(pageNumber, pageSize));

            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Отримання архіву видалених семестрів (Admin, Director)")
        .Produces<PagedResponse<SemesterResponse>>(StatusCodes.Status200OK);

        app.MapGet("/api/semesters/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetSemesterByIdQuery(id);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireAuthorization() 
        .WithTags(ReferenceTag)
        .WithSummary("Отримання деталей семестру за ID (Всі ролі)")
        .Produces<SemesterResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPost("/api/semesters", async (
            [FromBody] CreateSemesterRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateSemesterCommand(
                request.SemesterName,
                request.StartDate,
                request.EndDate);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                            ? HandleError(result.FirstError)
                            : Results.Ok(new { SemesterId = result.Value });
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Створення нового семестру (Admin, Director)")
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPut("/api/semesters/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateSemesterRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateSemesterCommand(
                id,
                request.SemesterName,
                request.StartDate,
                request.EndDate,
                request.RowVersionBase64);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                            ? HandleError(result.FirstError)
                            : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Оновлення семестру з перевіркою версій (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapDelete("/api/semesters/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] DeleteSemesterRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteSemesterCommand(
                id,
                request.RowVersionBase64);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                            ? HandleError(result.FirstError)
                            : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("М'яке видалення семестру (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPost("/api/semesters/{id:guid}/restore", async (
            [FromRoute] Guid id,
            [FromBody] RestoreSemesterRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RestoreSemesterCommand(
                id,
                request.RowVersionBase64);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                            ? HandleError(result.FirstError)
                            : Results.Ok();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Відновлення м'яко видаленого семестру (Admin, Director)")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}