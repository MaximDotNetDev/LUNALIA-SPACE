using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Operations.Lessons.CreateLesson;
using SchoolJournal.Application.Features.Operations.Lessons.DeleteLesson;
using SchoolJournal.Application.Features.Operations.Lessons.GetClassroomOccupancy;
using SchoolJournal.Application.Features.Operations.Lessons.GetLessonById;
using SchoolJournal.Application.Features.Operations.Lessons.GetLessonsByAssignment;
using SchoolJournal.Application.Features.Operations.Lessons.GetScheduleByDateRange;
using SchoolJournal.Application.Features.Operations.Lessons.RescheduleLesson;
using SchoolJournal.Application.Features.Operations.Lessons.UpdateLessonTopicAndHomework;
using SchoolJournal.Contracts.DTOs.Operations.Lessons;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Operations;

internal static class LessonsEndpoint
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
            ErrorOr.ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(statusCode: statusCode, title: error.Description);
    }

    public static void MapLessons(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/lessons/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetLessonByIdQuery(id);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireAuthorization() 
        .WithTags(OperationsTag)
        .WithSummary("Отримання деталей уроку за ID з урахуванням прав доступу (Всі ролі)")
        .Produces<LessonResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden);

        app.MapGet("/api/lessons", async (
        [FromQuery] Guid assignmentId,
        ISender sender,
        CancellationToken cancellationToken) =>
        {
            GetLessonsByAssignmentQuery query = new(assignmentId);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
    .RequireAuthorization()
    .WithTags(OperationsTag)
    .WithSummary("Отримання списку уроків за AssignmentId з матричною перевіркою ролей")
.Produces<IReadOnlyCollection<LessonResponse>>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status401Unauthorized)
    .ProducesProblem(StatusCodes.Status403Forbidden);

        app.MapGet("/api/lessons/schedule", async (
            [FromQuery] DateTimeOffset startDate,
            [FromQuery] DateTimeOffset endDate,
            [FromQuery] Guid semesterId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            GetScheduleByDateRangeQuery query = new(startDate, endDate, semesterId);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireAuthorization()
        .WithTags(OperationsTag)
        .WithSummary("Отримання розкладу уроків за період (з автоматичною фільтрацією по ролі та ID)")
.Produces<IReadOnlyCollection<LessonResponse>>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status401Unauthorized);

        app.MapGet("/api/lessons/room/{roomId:guid}/occupancy", async (
            [FromRoute] Guid roomId,
            [FromQuery] DateTimeOffset date,
            [FromQuery] Guid? periodId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            GetClassroomOccupancyQuery query = new(roomId, date, periodId);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireAuthorization()
        .WithTags(OperationsTag)
        .WithSummary("Перевірка зайнятості кабінету (повертає уроки з урахуванням ролі)")
        .Produces<IReadOnlyCollection<LessonResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    
        app.MapPost("/api/lessons", async (
            [FromBody] CreateLessonRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateLessonCommand(
                request.AssignmentId,
                request.LessonDate,
                request.LessonTopic,
                request.Homework,
                request.LessonTypeId,
                request.PeriodId,
                request.RoomId,
                request.SemesterId);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(new { LessonId = result.Value });
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
        .WithTags(OperationsTag)
        .WithSummary("Створення нового уроку з перевіркою власності (Admin, Director, Teacher)")
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden);

        app.MapPut("/api/lessons/{id:guid}/topic-and-homework", async (
            [FromRoute] Guid id,
            [FromBody] UpdateLessonTopicAndHomeworkRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateLessonTopicAndHomeworkCommand(
                id,
                request.LessonTopic,
                request.Homework,
                request.RowVersionBase64);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
        .WithTags(OperationsTag)
        .WithSummary("Оновлення теми та домашнього завдання уроку (Admin, Director, Teacher)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
.ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPatch("/api/lessons/{id:guid}/reschedule", async (
            [FromRoute] Guid id,
            [FromBody] RescheduleLessonRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RescheduleLessonCommand(
                id,
                request.LessonDate,
                request.PeriodId,
                request.RoomId,
                request.RowVersionBase64);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
        .WithTags(OperationsTag)
        .WithSummary("Перенесення уроку на іншу дату, час або в інший кабінет")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
.ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapDelete("/api/lessons/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] DeleteLessonRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteLessonCommand(
                id,
                request.RowVersionBase64);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
        .WithTags(OperationsTag)
        .WithSummary("М'яке видалення (скасування) уроку")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}