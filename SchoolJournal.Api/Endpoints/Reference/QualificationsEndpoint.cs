using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Reference.Qualification.CreateQualification;
using SchoolJournal.Application.Features.Reference.Qualification.DeleteQualification;
using SchoolJournal.Application.Features.Reference.Qualification.GetActiveQualifications;
using SchoolJournal.Application.Features.Reference.Qualification.GetDeletedQualifications;
using SchoolJournal.Application.Features.Reference.Qualification.GetQualificationById;
using SchoolJournal.Application.Features.Reference.Qualification.RestoreQualification;
using SchoolJournal.Application.Features.Reference.Qualification.UpdateQualification;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.Qualifications;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Reference;

internal static class QualificationsEndpoint
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

    public static void MapQualifications(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/qualifications", async (
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetActiveQualificationsQuery(
                new PageRequest(pageNumber, pageSize));

            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
.RequireAuthorization()
        .WithTags(ReferenceTag)
        .WithSummary("Отримання списку активних кваліфікацій (Всі ролі)")
        .Produces<PagedResponse<QualificationResponse>>(StatusCodes.Status200OK);

        app.MapGet("/api/qualifications/archive", async (
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetDeletedQualificationsQuery(
                new PageRequest(pageNumber, pageSize));

            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Отримання архіву видалених кваліфікацій (Admin, Director)")
        .Produces<PagedResponse<QualificationResponse>>(StatusCodes.Status200OK);

        app.MapGet("/api/qualifications/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetQualificationByIdQuery(id);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireAuthorization()
        .WithTags(ReferenceTag)
        .WithSummary("Отримання деталей кваліфікації за ID (Всі ролі)")
        .Produces<QualificationResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPost("/api/qualifications", async (
            [FromBody] CreateQualificationRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateQualificationCommand(request.QualificationName);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(new { QualificationId = result.Value });
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Створення нової кваліфікації (Admin, Director)")
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPut("/api/qualifications/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateQualificationRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateQualificationCommand(
                id,
                request.QualificationName,
                request.RowVersionBase64);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Оновлення кваліфікації з перевіркою версій (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapDelete("/api/qualifications/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] DeleteQualificationRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteQualificationCommand(
                id,
                request.RowVersionBase64);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("М'яке видалення кваліфікації (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPost("/api/qualifications/{id:guid}/restore", async (
            [FromRoute] Guid id,
            [FromBody] RestoreQualificationRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RestoreQualificationCommand(
                id,
                request.RowVersionBase64);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(ReferenceTag)
        .WithSummary("Відновлення м'яко видаленої кваліфікації (Admin, Director)")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}