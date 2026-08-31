using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Application.Features.Communications.Announcements.CreateAnnouncement;
using SchoolJournal.Application.Features.Communications.Announcements.DeleteAnnouncement;
using SchoolJournal.Application.Features.Communications.Announcements.GetActiveAnnouncements;
using SchoolJournal.Application.Features.Communications.Announcements.GetAnnouncementById;
using SchoolJournal.Application.Features.Communications.Announcements.GetAnnouncementsList;
using SchoolJournal.Application.Features.Communications.Announcements.ToggleAnnouncementStatus;
using SchoolJournal.Application.Features.Communications.Announcements.UpdateAnnouncement;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Communications.Announcements;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Communications;

internal static class AnnouncementsEndpoint
{
    private const string CommunicationsTag = "Communications";

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

    public static void MapAnnouncements(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/announcements", async (
            [FromBody] CreateAnnouncementRequest request,
            ISender sender,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken) =>
        {
            var authorId = currentUserService.GetUserId();
            if (authorId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var command = new CreateAnnouncementCommand(
                request.Title,
                request.Content,
                authorId,
                request.ExpirationDate);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(new { AnnouncementId = result.Value });
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(CommunicationsTag)
        .WithSummary("Створення нового оголошення (Admin, Director)")
.Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden);

        app.MapPut("/api/announcements/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateAnnouncementRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateAnnouncementCommand(
                id,
                request.Title,
                request.Content,
                request.ExpirationDate,
                request.RowVersionBase64);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(CommunicationsTag)
        .WithSummary("Оновлення оголошення з перевіркою RowVersion (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPatch("/api/announcements/{id:guid}/toggle", async (
            [FromRoute] Guid id,
            [FromBody] ToggleAnnouncementStatusRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new ToggleAnnouncementStatusCommand(id, request.RowVersionBase64);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(CommunicationsTag)
        .WithSummary("Активація/Деактивація оголошення (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapDelete("/api/announcements/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] DeleteAnnouncementRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteAnnouncementCommand(id, request.RowVersionBase64);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(CommunicationsTag)
        .WithSummary("М'яке видалення оголошення (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapGet("/api/announcements", async (
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetActiveAnnouncementsQuery(new PageRequest(pageNumber, pageSize)), cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithTags(CommunicationsTag)
        .WithSummary("Список активних оголошень (Всі ролі)");

        app.MapGet("/api/announcements/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetAnnouncementByIdQuery(id), cancellationToken).ConfigureAwait(false);
            return result.IsError ? HandleError(result.FirstError) : Results.Ok(result.Value);
        })
        .RequireAuthorization()
        .WithTags(CommunicationsTag)
        .WithSummary("Деталі оголошення (Всі ролі)");

        app.MapGet("/api/announcements/admin", async (
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize,
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] Guid? authorId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAnnouncementsListQuery(new PageRequest(pageNumber, pageSize), search, isActive, authorId);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(CommunicationsTag)
        .WithSummary("Управління оголошеннями (Admin, Director)");
    }
}