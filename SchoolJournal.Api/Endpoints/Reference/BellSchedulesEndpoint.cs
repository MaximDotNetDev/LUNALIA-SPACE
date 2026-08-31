using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Reference.BellSchedule.CreateBellSchedule;
using SchoolJournal.Application.Features.Reference.BellSchedule.DeleteBellSchedule;
using SchoolJournal.Application.Features.Reference.BellSchedule.GetActiveBellSchedules;
using SchoolJournal.Application.Features.Reference.BellSchedule.GetBellScheduleById;
using SchoolJournal.Application.Features.Reference.BellSchedule.UpdateBellSchedule;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.BellSchedules;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Reference;

internal static class BellSchedulesEndpoint
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

    public static void MapBellSchedules(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/bell-schedules", async (
            [FromBody] CreateBellScheduleRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateBellScheduleCommand(
                request.LessonNumber,
                request.StartTime,
                request.EndTime);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(new { ScheduleId = result.Value });
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Створення нового розкладу дзвінків (Admin, Director)")
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPut("/api/bell-schedules/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateBellScheduleRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateBellScheduleCommand(
                id,
                request.LessonNumber,
                request.StartTime,
                request.EndTime);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Оновлення розкладу дзвінків (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapDelete("/api/bell-schedules/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteBellScheduleCommand(id);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Видалення розкладу дзвінків (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapGet("/api/bell-schedules", async (
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetActiveBellSchedulesQuery(
                new PageRequest(pageNumber, pageSize));

            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithTags(ReferenceTag)
        .WithSummary("Отримання списку активних розкладів (Всі ролі)")
        .Produces<PagedResponse<BellScheduleResponse>>(StatusCodes.Status200OK);

        app.MapGet("/api/bell-schedules/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetBellScheduleByIdQuery(id);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireAuthorization()
        .WithTags(ReferenceTag)
        .WithSummary("Отримання деталей розкладу за ID (Всі ролі)")
        .Produces<BellScheduleResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}