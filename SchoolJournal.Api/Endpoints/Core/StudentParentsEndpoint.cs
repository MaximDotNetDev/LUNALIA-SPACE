using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Core.StudentParents.AssignParentToStudent;
using SchoolJournal.Application.Features.Core.StudentParents.GetParentsByStudentId;
using SchoolJournal.Application.Features.Core.StudentParents.GetStudentParentById;
using SchoolJournal.Application.Features.Core.StudentParents.GetStudentsByParentId;
using SchoolJournal.Application.Features.Core.StudentParents.RemoveParentFromStudent;
using SchoolJournal.Application.Features.Core.StudentParents.RestoreStudentParent;
using SchoolJournal.Application.Features.Core.StudentParents.UpdateStudentParentRole;
using SchoolJournal.Contracts.DTOs.Core.StudentParents;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Core;

internal static class StudentParentsEndpoint
{
    private const string CoreTag = "StudentParents";

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

    public static void MapStudentParents(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/student-parents", async (
            [FromBody] AssignParentToStudentRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new AssignParentToStudentCommand(
                request.StudentId,
                request.ParentId,
                request.Role);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(new { StudentParentId = result.Value });
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(CoreTag)
        .WithSummary("Призначення батьків учню (Admin, Director)")
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
.ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPut("/api/student-parents/{id:guid}/role", async (
            [FromRoute] Guid id,
            [FromBody] UpdateStudentParentRoleRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateStudentParentRoleCommand(id, request.Role);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(CoreTag)
        .WithSummary("Оновлення ролі у зв'язку батьки-учень (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapDelete("/api/student-parents/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RemoveParentFromStudentCommand(id);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(CoreTag)
        .WithSummary("М'яке видалення зв'язку батьки-учень (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
.ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPost("/api/student-parents/{id:guid}/restore", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RestoreStudentParentCommand(id);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(CoreTag)
        .WithSummary("Відновлення м'яко видаленого зв'язку батьки-учень (Admin, Director)")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
.ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapGet("/api/students/{studentId:guid}/parents", async (
            [FromRoute] Guid studentId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetParentsByStudentIdQuery(studentId);

            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(CoreTag)
        .WithSummary("Отримання списку батьків для конкретного учня (Admin, Director)")
        .Produces<IEnumerable<StudentParentDetailResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapGet("/api/parents/{parentId:guid}/students", async (
            [FromRoute] Guid parentId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetStudentsByParentIdQuery(parentId);

            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(CoreTag)
        .WithSummary("Отримання списку учнів для конкретних батьків (Admin, Director)")
        .Produces<IEnumerable<ParentStudentDetailResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
.ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapGet("/api/student-parents/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetStudentParentByIdQuery(id);

            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(CoreTag)
        .WithSummary("Отримання деталей зв'язку за ID (Admin, Director)")
        .Produces<StudentParentResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}