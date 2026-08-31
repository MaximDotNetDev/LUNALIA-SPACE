using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Infrastructure.OutboxMessages.MarkProcessed;
using SchoolJournal.Domain.Enums.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolJournal.Application.Features.Infrastructure.OutboxMessages.MarkFailed;
using SchoolJournal.Application.Features.Infrastructure.OutboxMessages.PurgeOld;
using SchoolJournal.Contracts.DTOs.Infrastructure.OutboxMessages;
using SchoolJournal.Application.Features.Infrastructure.OutboxMessages.GetList;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Application.Features.Infrastructure.OutboxMessages.GetById;

namespace SchoolJournal.Api.Endpoints.Infrastructure;

internal static class OutboxMessagesEndpoint
{
    private const string InfrastructureTag = "Infrastructure (Outbox)";

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

    public static void MapOutboxMessages(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/outbox", async (
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize,
            [FromQuery] string? type,
            [FromQuery] bool? hasError,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetOutboxMessagesListQuery(
                new PageRequest(pageNumber, pageSize),
                type,
                hasError);

            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
        .RequireRoles(RoleType.Admin)
        .WithTags(InfrastructureTag)
        .WithSummary("Отримання списку Outbox-повідомлень (Admin)")
.Produces<PagedResponse<OutboxMessageResponse>>(StatusCodes.Status200OK);

        app.MapGet("/api/outbox/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetOutboxMessageByIdQuery(id);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireRoles(RoleType.Admin)
        .WithTags(InfrastructureTag)
        .WithSummary("Отримання деталей Outbox-повідомлення за ID (Admin)")
        .Produces<OutboxMessageResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPut("/api/outbox/{id:guid}/process", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new MarkOutboxMessageProcessedCommand(id);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin)
        .WithTags(InfrastructureTag)
        .WithSummary("Позначити Outbox-повідомлення як оброблене вручну (Admin)")
        .Produces(StatusCodes.Status204NoContent)
.ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPut("/api/outbox/{id:guid}/fail", async (
            [FromRoute] Guid id,
            [FromBody] MarkOutboxMessageFailedRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new MarkOutboxMessageFailedCommand(
                id,
                request.ErrorMessage,
                request.StopRetrying);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin)
        .WithTags(InfrastructureTag)
        .WithSummary("Зафіксувати помилку обробки повідомлення (Admin)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
.ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapDelete("/api/outbox/purge", async (
            [FromBody] PurgeOutboxMessagesRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new PurgeOldOutboxMessagesCommand(request.OlderThanDays);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(new { DeletedCount = result.Value });
        })
        .RequireRoles(RoleType.Admin)
        .WithTags(InfrastructureTag)
        .WithSummary("Очищення старих оброблених повідомлень (Admin)")
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}