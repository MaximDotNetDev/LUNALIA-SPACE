using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Operations.FixedSchedules.CreateFixedSchedule;
using SchoolJournal.Application.Features.Operations.FixedSchedules.DeleteFixedSchedule;
using SchoolJournal.Application.Features.Operations.FixedSchedules.GetFixedScheduleById;
using SchoolJournal.Application.Features.Operations.FixedSchedules.GetFixedSchedulesByAssignmentId;
using SchoolJournal.Application.Features.Operations.FixedSchedules.GetFixedSchedulesByDay;
using SchoolJournal.Application.Features.Operations.FixedSchedules.GetFixedSchedulesByRoomId;
using SchoolJournal.Application.Features.Operations.FixedSchedules.UpdateFixedSchedule;
using SchoolJournal.Contracts.DTOs.Operations.FixedSchedules;
using SchoolJournal.Domain.Enums;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Operations;

internal static class FixedSchedulesEndpoint
{
    private const string OperationsTag = "FixedSchedules";

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

    public static void MapFixedSchedules(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/fixed-schedules", async (
            [FromBody] CreateFixedScheduleRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateFixedScheduleCommand(
                (SchoolDayOfWeek)request.DayOfWeek,
                request.PeriodId,
                request.AssignmentId,
                request.RoomId);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(new { ScheduleId = result.Value });
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(OperationsTag)
        .WithSummary("Створення нового елементу сталого розкладу (Admin, Director)")
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPut("/api/fixed-schedules/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateFixedScheduleRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateFixedScheduleCommand(
                id,
                (SchoolDayOfWeek)request.DayOfWeek,
                request.PeriodId,
                request.AssignmentId,
                request.RoomId,
                request.RowVersionBase64);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(OperationsTag)
        .WithSummary("Оновлення запису сталого розкладу з перевіркою версій (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapDelete("/api/fixed-schedules/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] DeleteFixedScheduleRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteFixedScheduleCommand(id, request.RowVersionBase64);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(OperationsTag)
        .WithSummary("М'яке видалення запису розкладу (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapGet("/api/fixed-schedules/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetFixedScheduleByIdQuery(id);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireAuthorization()
        .WithTags(OperationsTag)
        .WithSummary("Отримання деталей запису розкладу за ID (Всі ролі)")
        .Produces<FixedScheduleResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapGet("/api/fixed-schedules/assignment/{assignmentId:guid}", async (
            [FromRoute] Guid assignmentId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetFixedSchedulesByAssignmentIdQuery(assignmentId);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireAuthorization()
        .WithTags(OperationsTag)
        .WithSummary("Отримання списку елементів розкладу за навчальним призначенням (Всі ролі)")
        .Produces<IEnumerable<FixedScheduleResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        app.MapGet("/api/fixed-schedules/room/{roomId:guid}", async (
            [FromRoute] Guid roomId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetFixedSchedulesByRoomIdQuery(roomId);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireAuthorization()
        .WithTags(OperationsTag)
        .WithSummary("Отримання списку елементів розкладу за кабінетом (Всі ролі)")
        .Produces<IEnumerable<FixedScheduleResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        app.MapGet("/api/fixed-schedules/day/{dayOfWeek:int}", async (
            [FromRoute] int dayOfWeek,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetFixedSchedulesByDayQuery(dayOfWeek);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireAuthorization()
        .WithTags(OperationsTag)
        .WithSummary("Отримання списку елементів розкладу за днем тижня (Всі ролі)")
        .Produces<IEnumerable<FixedScheduleResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}