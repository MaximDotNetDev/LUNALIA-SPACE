using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Reference.GradeType.CreateGradeType;
using SchoolJournal.Application.Features.Reference.GradeType.DeleteGradeType;
using SchoolJournal.Application.Features.Reference.GradeType.GetActiveGradeTypes;
using SchoolJournal.Application.Features.Reference.GradeType.GetGradeTypeById;
using SchoolJournal.Application.Features.Reference.GradeType.GetGradeTypesArchive;
using SchoolJournal.Application.Features.Reference.GradeType.RestoreGradeType;
using SchoolJournal.Application.Features.Reference.GradeType.UpdateGradeType;
using SchoolJournal.Application.Features.Reference.LessonType.DeleteLessonType;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.GradeTypes;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Reference;

internal static class GradeTypesEndpoint
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

    public static void MapGradeTypes(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/gradetypes", async (
            [FromBody] CreateGradeTypeRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateGradeTypeCommand(request.TypeName);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(new { GradeTypeId = result.Value });
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Створення нового типу оцінки (Admin, Director)")
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPut("/api/gradetypes/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateGradeTypeRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateGradeTypeCommand(id, request.TypeName);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Оновлення типу оцінки (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapDelete("/api/gradetypes/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteGradeTypeCommand(id);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("М'яке видалення типу оцінки (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPost("/api/gradetypes/{id:guid}/restore", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RestoreGradeTypeCommand(id);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Відновлення м'яко видаленого типу оцінки (Admin, Director)")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapGet("/api/gradetypes", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetActiveGradeTypesQuery(), cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithTags(ReferenceTag)
        .WithSummary("Отримання активних типів оцінок для випадаючих списків (Всі авторизовані)")
        .Produces<IEnumerable<GradeTypeResponse>>(StatusCodes.Status200OK);

        app.MapGet("/api/gradetypes/archive", async (
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetGradeTypesArchiveQuery(new PageRequest(pageNumber, pageSize));
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Архів усіх типів оцінок (Admin, Director)")
        .Produces<PagedResponse<GradeTypeResponse>>(StatusCodes.Status200OK);

        app.MapGet("/api/gradetypes/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetGradeTypeByIdQuery(id), cancellationToken).ConfigureAwait(false);
            return result.IsError ? HandleError(result.FirstError) : Results.Ok(result.Value);
        })
        .RequireAuthorization()
        .WithTags(ReferenceTag)
        .WithSummary("Деталі типу оцінки (Всі авторизовані)")
        .Produces<GradeTypeResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapDelete("/api/lessontypes/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteLessonTypeCommand(id);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("М'яке видалення типу уроку (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}