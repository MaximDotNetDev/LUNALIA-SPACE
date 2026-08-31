using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Reference.Position.CreatePosition;
using SchoolJournal.Application.Features.Reference.Position.DeletePosition;
using SchoolJournal.Application.Features.Reference.Position.GetPositionById;
using SchoolJournal.Application.Features.Reference.Position.GetPositionsList;
using SchoolJournal.Application.Features.Reference.Position.UpdatePosition;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.Positions;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Reference;

internal static class PositionsEndpoint
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

    public static void MapPositions(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/positions", async (
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetPositionsListQuery(new PageRequest(pageNumber, pageSize));
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithTags(ReferenceTag)
        .WithSummary("Отримання списку посад з пагінацією (Всі ролі)")
        .Produces<PagedResponse<PositionResponse>>(StatusCodes.Status200OK);

        app.MapPost("/api/positions", async (
            [FromBody] CreatePositionRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreatePositionCommand(request.PositionName);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(new { PositionId = result.Value });
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Створення нової посади (Admin, Director)")
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
.ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapGet("/api/positions/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetPositionByIdQuery(id);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireAuthorization()
        .WithTags(ReferenceTag)
        .WithSummary("Отримання деталей посади за ID (Всі авторизовані ролі)")
        .Produces<PositionResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPut("/api/positions/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdatePositionRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdatePositionCommand(id, request.PositionName);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Оновлення назви посади (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapDelete("/api/positions/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new DeletePositionCommand(id);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("М'яке видалення посади (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}