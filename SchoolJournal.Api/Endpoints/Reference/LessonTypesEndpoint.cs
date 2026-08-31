using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Reference.LessonType.CreateLessonType;
using SchoolJournal.Application.Features.Reference.LessonType.RestoreLessonType;
using SchoolJournal.Application.Features.Reference.LessonType.UpdateLessonType;
using SchoolJournal.Contracts.DTOs.Reference.LessonTypes;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Reference;

internal static class LessonTypesEndpoint
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

    public static void MapLessonTypes(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/lessontypes", async (
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new Application.Features.Reference.LessonType.GetActiveLessonTypes.GetActiveLessonTypesQuery(
                new Contracts.Common.PageRequest(pageNumber, pageSize));

            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
            return Results.Ok(result.Value);
        })
        .RequireAuthorization()
        .WithTags(ReferenceTag)
        .WithSummary("Отримання списку активних типів уроків (Всі ролі)")
        .Produces<Contracts.Common.PagedResponse<LessonTypeResponse>>(StatusCodes.Status200OK);

        app.MapGet("/api/lessontypes/archive", async (
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new Application.Features.Reference.LessonType.GetDeletedLessonTypes.GetDeletedLessonTypesQuery(
                new Contracts.Common.PageRequest(pageNumber, pageSize));

            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
            return Results.Ok(result.Value);
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Отримання архіву видалених типів уроків (Admin, Director)")
        .Produces<Contracts.Common.PagedResponse<LessonTypeResponse>>(StatusCodes.Status200OK);

        app.MapGet("/api/lessontypes/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new Application.Features.Reference.LessonType.GetLessonTypeById.GetLessonTypeByIdQuery(id);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireAuthorization()
        .WithTags(ReferenceTag)
        .WithSummary("Отримання деталей типу уроку за ID (Всі ролі)")
        .Produces<LessonTypeResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPost("/api/lessontypes", async (
            [FromBody] CreateLessonTypeRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateLessonTypeCommand(request.TypeName);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(new { LessonTypeId = result.Value });
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Створення нового типу уроку (Admin, Director)")
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
.ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPut("/api/lessontypes/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateLessonTypeRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateLessonTypeCommand(id, request.TypeName);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Оновлення типу уроку (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status403Forbidden);

        app.MapPost("/api/lessontypes/{id:guid}/restore", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RestoreLessonTypeCommand(id);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Відновлення м'яко видаленого типу уроку (Admin, Director)")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}