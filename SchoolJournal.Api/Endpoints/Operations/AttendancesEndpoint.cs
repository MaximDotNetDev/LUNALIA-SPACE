using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Operations.Attendance.BulkRecordAttendance;
using SchoolJournal.Application.Features.Operations.Attendance.GetAttendanceById;
using SchoolJournal.Application.Features.Operations.Attendance.GetLessonAttendanceRegister;
using SchoolJournal.Application.Features.Operations.Attendance.GetStudentAttendanceHistory;
using SchoolJournal.Application.Features.Operations.Attendance.GetStudentAttendanceStats;
using SchoolJournal.Application.Features.Operations.Attendance.RecordAttendance;
using SchoolJournal.Application.Features.Operations.Attendance.SoftDeleteAttendance;
using SchoolJournal.Application.Features.Operations.Attendance.UpdateAttendance;
using SchoolJournal.Contracts.DTOs.Operations.Attendances;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Operations;

internal static class AttendancesEndpoint
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
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(statusCode: statusCode, title: error.Description);
    }

    public static void MapAttendances(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/attendances", RecordAttendanceAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
            .WithTags(OperationsTag)
            .WithSummary("Фіксація первинної відвідуваності студента на уроці (Admin, Director, Teacher)")
            .Produces<object>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPut("/api/attendances/{id:guid}", UpdateAttendanceAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
            .WithTags(OperationsTag)
            .WithSummary("Оновлення відвідуваності з перевіркою версій та прав власності (Admin, Director, Teacher)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPost("/api/attendances/bulk", BulkRecordAttendanceAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
            .WithTags(OperationsTag)
            .WithSummary("Масове проставлення та оновлення відомості відвідуваності уроку (Admin, Director, Teacher)")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapDelete("/api/attendances/{id:guid}", SoftDeleteAttendanceAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
            .WithTags(OperationsTag)
            .WithSummary("М'яке видалення запису відвідуваності з перевіркою версії та прав (Admin, Director, Teacher)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapGet("/api/attendances/{id:guid}", GetAttendanceByIdAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher, RoleType.Student, RoleType.Parent)
            .WithTags(OperationsTag)
            .WithSummary("Отримання деталей конкретного запису відвідуваності з багаторанговою перевіркою прав доступу (Всі ролі)")
            .Produces<AttendanceResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapGet("/api/attendances/lessons/{lessonId:guid}/register", GetLessonAttendanceRegisterAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
            .WithTags(OperationsTag)
            .WithSummary("Отримання повної журнальної відомості відвідуваності для уроку (Admin, Director, Teacher)")
            .Produces<LessonAttendanceRegisterResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapGet("/api/attendances/students/{studentId:guid}/history", GetStudentAttendanceHistoryAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher, RoleType.Student, RoleType.Parent)
            .WithTags(OperationsTag)
            .WithSummary("Отримання повної хронологічної історії відвідуваності конкретного студента за вказаний період (Всі ролі)")
            .Produces<StudentAttendanceHistoryResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapGet("/api/attendances/students/{studentId:guid}/stats", GetStudentAttendanceStatsAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher, RoleType.Student, RoleType.Parent)
            .WithTags(OperationsTag)
            .WithSummary("Отримання агрегованої аналітичної статистики відвідуваності студента в загальному та в розрізі предметів (Всі ролі)")
            .Produces<StudentAttendanceStatsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> RecordAttendanceAsync(
        [FromBody] RecordAttendanceRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new RecordAttendanceCommand(
            request.LessonId,
            request.StudentId,
            request.Status,
            request.Comment);

        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

        return result.IsError
            ? HandleError(result.FirstError)
            : Results.Ok(new { AttendanceId = result.Value });
    }

    private static async Task<IResult> UpdateAttendanceAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateAttendanceRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateAttendanceCommand(
            id,
            request.Status,
            request.Comment,
            request.RowVersionBase64);

        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

        return result.IsError
            ? HandleError(result.FirstError)
            : Results.NoContent();
    }

    private static async Task<IResult> BulkRecordAttendanceAsync(
        [FromBody] BulkRecordAttendanceRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new BulkRecordAttendanceCommand(
            request.LessonId,
            request.Students);

        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

        return result.IsError
            ? HandleError(result.FirstError)
            : Results.Ok();
    }

    private static async Task<IResult> SoftDeleteAttendanceAsync(
        [FromRoute] Guid id,
        [FromBody] DeleteAttendanceRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new SoftDeleteAttendanceCommand(
            id,
            request.RowVersionBase64);

        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

        return result.IsError
            ? HandleError(result.FirstError)
            : Results.NoContent();
    }

    private static async Task<IResult> GetAttendanceByIdAsync(
        [FromRoute] Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetAttendanceByIdQuery(id);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

        return result.IsError
            ? HandleError(result.FirstError)
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetLessonAttendanceRegisterAsync(
        [FromRoute] Guid lessonId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetLessonAttendanceRegisterQuery(lessonId);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

        return result.IsError
            ? HandleError(result.FirstError)
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetStudentAttendanceHistoryAsync(
        [FromRoute] Guid studentId,
        [FromQuery] DateTimeOffset? startDate,
        [FromQuery] DateTimeOffset? endDate,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetStudentAttendanceHistoryQuery(studentId, startDate, endDate);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

        return result.IsError
            ? HandleError(result.FirstError)
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetStudentAttendanceStatsAsync(
        [FromRoute] Guid studentId,
        [FromQuery] DateTimeOffset? startDate,
        [FromQuery] DateTimeOffset? endDate,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetStudentAttendanceStatsQuery(studentId, startDate, endDate);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

        return result.IsError
            ? HandleError(result.FirstError)
            : Results.Ok(result.Value);
    }
}