using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Reference.PedagogicalTitles.CreatePedagogicalTitle;
using SchoolJournal.Application.Features.Reference.PedagogicalTitles.DeletePedagogicalTitle;
using SchoolJournal.Application.Features.Reference.PedagogicalTitles.GetActivePagedPedagogicalTitles;
using SchoolJournal.Application.Features.Reference.PedagogicalTitles.GetActivePedagogicalTitles;
using SchoolJournal.Application.Features.Reference.PedagogicalTitles.GetDeletedPagedPedagogicalTitles;
using SchoolJournal.Application.Features.Reference.PedagogicalTitles.GetPedagogicalTitleById;
using SchoolJournal.Application.Features.Reference.PedagogicalTitles.RestorePedagogicalTitle;
using SchoolJournal.Application.Features.Reference.PedagogicalTitles.UpdatePedagogicalTitle;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.PedagogicalTitles;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Reference;

internal static class PedagogicalTitlesEndpoint
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

    public static void MapPedagogicalTitles(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/pedagogical-titles", async (
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetActivePagedPedagogicalTitlesQuery(
                new PageRequest(pageNumber > 0 ? pageNumber : 1, pageSize > 0 ? pageSize : 10));

            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
            return Results.Ok(result.Value);
        })
        .RequireAuthorization()
        .WithTags(ReferenceTag)
        .WithSummary("Отримання списку активних педагогічних звань з пагінацією (Всі ролі)")
        .Produces<PagedResponse<PedagogicalTitleResponse>>(StatusCodes.Status200OK);

        app.MapGet("/api/pedagogical-titles/archive", async (
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetDeletedPagedPedagogicalTitlesQuery(
                new PageRequest(pageNumber > 0 ? pageNumber : 1, pageSize > 0 ? pageSize : 10));

            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
            return Results.Ok(result.Value);
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Отримання архіву видалених звань з пагінацією (Admin, Director)")
        .Produces<PagedResponse<PedagogicalTitleResponse>>(StatusCodes.Status200OK);

        app.MapGet("/api/pedagogical-titles/active", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetActivePedagogicalTitlesQuery();
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return Results.Ok(result.Value);
        })
        .RequireAuthorization()
        .WithTags(ReferenceTag)
        .WithSummary("Отримання списку активних педагогічних звань для Dropdown (Всі ролі)")
        .Produces<IEnumerable<PedagogicalTitleResponse>>(StatusCodes.Status200OK);

        app.MapGet("/api/pedagogical-titles/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetPedagogicalTitleByIdQuery(id);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireAuthorization()
        .WithTags(ReferenceTag)
        .WithSummary("Отримання деталей педагогічного звання за ID (Всі ролі)")
        .Produces<PedagogicalTitleResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPost("/api/pedagogical-titles", async (
            [FromBody] CreatePedagogicalTitleRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreatePedagogicalTitleCommand(request.TitleName);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(new { TitleId = result.Value });
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Створення нового педагогічного звання (Admin, Director)")
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
.ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPut("/api/pedagogical-titles/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdatePedagogicalTitleRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdatePedagogicalTitleCommand(
                id,
                request.TitleName);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Оновлення педагогічного звання (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
.ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapDelete("/api/pedagogical-titles/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new DeletePedagogicalTitleCommand(id);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("М'яке видалення педагогічного звання (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
.ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPost("/api/pedagogical-titles/{id:guid}/restore", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RestorePedagogicalTitleCommand(id);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Відновлення м'яко видаленого педагогічного звання (Admin, Director)")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}