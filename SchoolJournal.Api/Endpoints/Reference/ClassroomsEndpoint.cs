using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Reference.Classroom.CreateClassroom;
using SchoolJournal.Contracts.DTOs.Reference.Classrooms;
using SchoolJournal.Domain.Enums.Identity;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Application.Features.Reference.Classroom.UpdateClassroom;
using SchoolJournal.Application.Features.Reference.Classroom.DeleteClassroom;
using SchoolJournal.Application.Features.Reference.Classroom.RestoreClassroom;
using SchoolJournal.Application.Features.Reference.Classroom.GetActiveClassrooms;
using SchoolJournal.Application.Features.Reference.Classroom.GetDeletedClassrooms;
using SchoolJournal.Application.Features.Reference.Classroom.GetClassroomById;

namespace SchoolJournal.Api.Endpoints.Reference;

internal static class ClassroomsEndpoint
{
    private const string ReferenceTag = "Reference.Classrooms";

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

    public static void MapClassrooms(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/classrooms", async (
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize,
            [FromQuery] string? searchTerm,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetActiveClassroomsQuery(
                new PageRequest(pageNumber, pageSize), searchTerm);

            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithTags(ReferenceTag)
        .WithSummary("Отримання списку активних аудиторій (Всі ролі)")
        .Produces<PagedResponse<ClassroomResponse>>(StatusCodes.Status200OK);

        app.MapGet("/api/classrooms/archive", async (
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize,
            [FromQuery] string? searchTerm,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetDeletedClassroomsQuery(
                new PageRequest(pageNumber, pageSize), searchTerm);

            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Отримання архіву видалених аудиторій (Admin, Director)")
        .Produces<PagedResponse<ClassroomResponse>>(StatusCodes.Status200OK);

        app.MapGet("/api/classrooms/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetClassroomByIdQuery(id);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireAuthorization()
        .WithTags(ReferenceTag)
        .WithSummary("Отримання деталей аудиторії за ID (Всі ролі)")
        .Produces<ClassroomResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPost("/api/classrooms", async (
            [FromBody] CreateClassroomRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateClassroomCommand(
                request.RoomNumber,
                request.Name,
                request.Capacity);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(new { RoomId = result.Value });
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Створення нової аудиторії (Admin, Director)")
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
.ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPut("/api/classrooms/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateClassroomRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateClassroomCommand(
                id,
                request.RoomNumber,
                request.Name,
                request.Capacity,
                request.RowVersionBase64);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Оновлення аудиторії з перевіркою версій (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
.ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapDelete("/api/classrooms/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] DeleteClassroomRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteClassroomCommand(
                id,
                request.RowVersionBase64);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("М'яке видалення аудиторії (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
.ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPost("/api/classrooms/{id:guid}/restore", async (
            [FromRoute] Guid id,
            [FromBody] RestoreClassroomRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RestoreClassroomCommand(
                id,
                request.RowVersionBase64);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Відновлення м'яко видаленої аудиторії (Admin, Director)")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}