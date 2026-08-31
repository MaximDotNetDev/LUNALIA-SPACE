using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Core.Teachers.AssignTeacherUser;
using SchoolJournal.Application.Features.Core.Teachers.CreateTeacher;
using SchoolJournal.Application.Features.Core.Teachers.DeleteTeacher;
using SchoolJournal.Application.Features.Core.Teachers.GetTeacherById;
using SchoolJournal.Application.Features.Core.Teachers.GetTeacherByUserId;
using SchoolJournal.Application.Features.Core.Teachers.GetTeachersList;
using SchoolJournal.Application.Features.Core.Teachers.GetTeacherWorkloadSummary;
using SchoolJournal.Application.Features.Core.Teachers.ToggleTeacherStatus;
using SchoolJournal.Application.Features.Core.Teachers.UpdateTeacherAcademicInfo;
using SchoolJournal.Application.Features.Core.Teachers.UpdateTeacherProfile;
using SchoolJournal.Contracts.DTOs.Core.Teachers;
using SchoolJournal.Domain.Enums;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Core;

internal static class TeachersEndpoint
{
    private const string TeachersTag = "Teachers";

    public static void MapTeachers(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/teachers", GetTeachersListAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(TeachersTag)
            .WithSummary("Отримання списку вчителів з пагінацією та фільтрами (Admin, Director)")
            .Produces<SchoolJournal.Contracts.Common.PagedResponse<TeacherListItemResponse>>(StatusCodes.Status200OK);

        app.MapPost("/api/teachers", CreateTeacherAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(TeachersTag)
            .WithSummary("Створення нового профілю вчителя (Admin, Director)")
            .Produces<object>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        app.MapPut("/api/teachers/{id:guid}", UpdateTeacherProfileAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(TeachersTag)
            .WithSummary("Оновлення профілю вчителя (Admin, Director)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPut("/api/teachers/{id:guid}/academic-info", UpdateTeacherAcademicInfoAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(TeachersTag)
            .WithSummary("Оновлення академічних даних вчителя (Admin, Director)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPatch("/api/teachers/{id:guid}/user", AssignTeacherUserAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(TeachersTag)
            .WithSummary("Прив'язка облікового запису до вчителя (Admin, Director)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPatch("/api/teachers/{id:guid}/status", ToggleTeacherStatusAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(TeachersTag)
            .WithSummary("Активація/деактивація вчителя (Admin, Director)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapDelete("/api/teachers/{id:guid}", DeleteTeacherAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(TeachersTag)
            .WithSummary("М'яке видалення профілю вчителя (Admin, Director)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapGet("/api/teachers/{id:guid}", GetTeacherByIdAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(TeachersTag)
            .WithSummary("Отримання повної інформації про вчителя (Admin, Director)")
            .Produces<TeacherResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapGet("/api/teachers/by-user/{userId:guid}", GetTeacherByUserIdAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(TeachersTag)
            .WithSummary("Отримання інформації про вчителя за ідентифікатором користувача (Admin, Director)")
            .Produces<TeacherResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapGet("/api/teachers/workload-summary", GetTeacherWorkloadSummaryAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(TeachersTag)
            .WithSummary("Отримання звіту про навантаження вчителів (Admin, Director)")
            .Produces<IEnumerable<TeacherWorkloadResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetTeachersListAsync(
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        [FromQuery] string? searchTerm,
        [FromQuery] Guid? positionId,
        [FromQuery] bool? isActive,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetTeachersListQuery(
            new SchoolJournal.Contracts.Common.PageRequest(pageNumber, pageSize),
            searchTerm,
            positionId,
            isActive);

        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> CreateTeacherAsync(
        [FromBody] CreateTeacherRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<Gender>(request.Gender, true, out var gender))
        {
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Некоректне значення статі.");
        }

        var command = new CreateTeacherCommand(
            request.LastName,
            request.FirstName,
            request.MiddleName,
            request.Phone,
            request.Specialization,
            request.DateOfBirth,
            gender,
            request.Workload,
            request.EducationInfo,
            request.MeetLink,
            request.PositionId,
            request.QualificationId,
            request.PedagogicalTitleId,
            request.UserId);

        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

        return result.Match(
            teacherId => Results.Ok(new { TeacherId = teacherId }),
            errors => Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: errors[0].Description));
    }

    private static async Task<IResult> UpdateTeacherProfileAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateTeacherProfileRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<Gender>(request.Gender, true, out var gender))
        {
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Некоректне значення статі.");
        }

        var command = new UpdateTeacherProfileCommand(
            id,
            request.LastName,
            request.FirstName,
            request.MiddleName,
            request.Phone,
            request.Specialization,
            request.DateOfBirth,
            gender,
            request.EducationInfo,
            request.MeetLink,
            request.RowVersionBase64);

        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

        return result.Match(
            _ => Results.NoContent(),
            errors => errors[0].Type == ErrorOr.ErrorType.NotFound
                ? Results.NotFound(new { errors[0].Description })
                : Results.Problem(statusCode: StatusCodes.Status409Conflict, title: errors[0].Description));
    }

    private static async Task<IResult> UpdateTeacherAcademicInfoAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateTeacherAcademicInfoRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTeacherAcademicInfoCommand(
            id,
            request.PositionId,
            request.QualificationId,
            request.PedagogicalTitleId,
            request.Workload,
            request.RowVersionBase64);

        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

        return result.Match(
            _ => Results.NoContent(),
            errors => errors[0].Type == ErrorOr.ErrorType.NotFound
                ? Results.NotFound(new { errors[0].Description })
                : Results.Problem(statusCode: StatusCodes.Status409Conflict, title: errors[0].Description));
    }

    private static async Task<IResult> AssignTeacherUserAsync(
        [FromRoute] Guid id,
        [FromBody] AssignTeacherUserRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new AssignTeacherUserCommand(
            id,
            request.UserId,
            request.RowVersionBase64);

        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

        return result.Match(
            _ => Results.NoContent(),
            errors => errors[0].Type == ErrorOr.ErrorType.NotFound
                ? Results.NotFound(new { errors[0].Description })
                : Results.Problem(statusCode: StatusCodes.Status409Conflict, title: errors[0].Description));
    }

    private static async Task<IResult> ToggleTeacherStatusAsync(
        [FromRoute] Guid id,
        [FromBody] ToggleTeacherStatusRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new ToggleTeacherStatusCommand(
            id,
            request.IsActive,
            request.RowVersionBase64);

        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

        return result.Match(
            _ => Results.NoContent(),
            errors => errors[0].Type == ErrorOr.ErrorType.NotFound
                ? Results.NotFound(new { errors[0].Description })
                : Results.Problem(statusCode: StatusCodes.Status409Conflict, title: errors[0].Description));
    }

    private static async Task<IResult> DeleteTeacherAsync(
        [FromRoute] Guid id,
        [FromBody] DeleteTeacherRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new DeleteTeacherCommand(
            id,
            request.RowVersionBase64);

        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

        return result.Match(
            _ => Results.NoContent(),
            errors => errors[0].Type == ErrorOr.ErrorType.NotFound
                ? Results.NotFound(new { errors[0].Description })
                : Results.Problem(statusCode: StatusCodes.Status409Conflict, title: errors[0].Description));
    }

    private static async Task<IResult> GetTeacherByIdAsync(
        [FromRoute] Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetTeacherByIdQuery(id);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

        return result.Match(
            teacher => Results.Ok(teacher),
            errors => Results.Problem(statusCode: StatusCodes.Status404NotFound, title: errors[0].Description));
    }

    private static async Task<IResult> GetTeacherByUserIdAsync(
        [FromRoute] Guid userId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetTeacherByUserIdQuery(userId);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

        return result.Match(
            teacher => Results.Ok(teacher),
            errors => Results.Problem(statusCode: StatusCodes.Status404NotFound, title: errors[0].Description));
    }

    private static async Task<IResult> GetTeacherWorkloadSummaryAsync(
        [FromQuery] bool onlyActive,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetTeacherWorkloadSummaryQuery(onlyActive);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

        return Results.Ok(result.Value);
    }
}