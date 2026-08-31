using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Operations.TeachingAssignments.CreateTeachingAssignment;
using SchoolJournal.Application.Features.Operations.TeachingAssignments.UpdateTeachingAssignment;
using SchoolJournal.Application.Features.Operations.TeachingAssignments.ToggleTeachingAssignmentStatus;
using SchoolJournal.Application.Features.Operations.TeachingAssignments.DeleteTeachingAssignment;
using SchoolJournal.Application.Features.Operations.TeachingAssignments.GetTeachingAssignmentById;
using SchoolJournal.Application.Features.Operations.TeachingAssignments.GetTeachingAssignmentsByTeacherId;
using SchoolJournal.Application.Features.Operations.TeachingAssignments.GetTeachingAssignmentsByClassId;
using SchoolJournal.Application.Features.Operations.TeachingAssignments.GetTeachingAssignmentsBySubjectId;
using SchoolJournal.Contracts.DTOs.Operations.TeachingAssignments;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Operations;

internal static class TeachingAssignmentsEndpoint
{
    private const string OperationsTag = "Teaching Assignments";

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

    public static void MapTeachingAssignments(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/subjects/{subjectId:guid}/assignments", async (
            [FromRoute] Guid subjectId,
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTeachingAssignmentsBySubjectIdQuery(
                subjectId,
                new PageRequest(pageNumber, pageSize));

            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithTags(OperationsTag)
        .WithSummary("Отримання списку призначень для конкретного предмета (Всі ролі)")
        .Produces<PagedResponse<TeachingAssignmentResponse>>(StatusCodes.Status200OK);

        app.MapGet("/api/classes/{classId:guid}/assignments", async (
            [FromRoute] Guid classId,
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTeachingAssignmentsByClassIdQuery(
                classId,
                new PageRequest(pageNumber, pageSize));

            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithTags(OperationsTag)
        .WithSummary("Отримання списку призначень для конкретного класу (Всі ролі)")
        .Produces<PagedResponse<TeachingAssignmentResponse>>(StatusCodes.Status200OK);

        app.MapGet("/api/teachers/{teacherId:guid}/assignments", async (
            [FromRoute] Guid teacherId,
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTeachingAssignmentsByTeacherIdQuery(
                teacherId,
                new PageRequest(pageNumber, pageSize));

            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithTags(OperationsTag)
        .WithSummary("Отримання списку призначень для конкретного вчителя (Всі ролі)")
        .Produces<PagedResponse<TeachingAssignmentResponse>>(StatusCodes.Status200OK);

        app.MapGet("/api/teaching-assignments/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTeachingAssignmentByIdQuery(id);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireAuthorization()
        .WithTags(OperationsTag)
        .WithSummary("Отримання деталей призначення за ID (Всі ролі)")
        .Produces<TeachingAssignmentResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPost("/api/teaching-assignments", async (
            [FromBody] CreateTeachingAssignmentRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateTeachingAssignmentCommand(
                request.TeacherId,
                request.SubjectId,
                request.ClassId,
                request.SubgroupId);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(new { AssignmentId = result.Value });
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(OperationsTag)
        .WithSummary("Призначення вчителя на предмет для класу/підгрупи (Admin, Director)")
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPut("/api/teaching-assignments/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateTeachingAssignmentRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateTeachingAssignmentCommand(
                id,
                request.TeacherId,
                request.SubjectId,
                request.ClassId,
                request.SubgroupId,
                request.RowVersionBase64);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(OperationsTag)
        .WithSummary("Оновлення призначення вчителя з перевіркою версій (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPatch("/api/teaching-assignments/{id:guid}/toggle-status", async (
            [FromRoute] Guid id,
            [FromBody] ToggleTeachingAssignmentStatusRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new ToggleTeachingAssignmentStatusCommand(id, request.RowVersionBase64);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(OperationsTag)
        .WithSummary("Перемикання статусу активності призначення (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapDelete("/api/teaching-assignments/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] DeleteTeachingAssignmentRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteTeachingAssignmentCommand(id, request.RowVersionBase64);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(OperationsTag)
        .WithSummary("М'яке видалення призначення вчителя (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}