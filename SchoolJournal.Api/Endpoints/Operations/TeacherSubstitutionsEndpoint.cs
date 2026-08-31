using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Operations.TeacherSubstitution.CreateTeacherSubstitution;
using SchoolJournal.Application.Features.Operations.TeacherSubstitution.UpdateTeacherSubstitution;
using SchoolJournal.Application.Features.Operations.TeacherSubstitution.DeleteTeacherSubstitution;
using SchoolJournal.Application.Features.Operations.TeacherSubstitution.GetTeacherSubstitutionById;
using SchoolJournal.Application.Features.Operations.TeacherSubstitution.GetSubstitutionsByAssignmentId;
using SchoolJournal.Application.Features.Operations.TeacherSubstitution.GetSubstitutionsByTeacherId;
using SchoolJournal.Application.Features.Operations.TeacherSubstitution.GetActiveSubstitutions;
using SchoolJournal.Contracts.DTOs.Operations.TeacherSubstitutions;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Operations;

internal static class TeacherSubstitutionsEndpoint
{
    private const string OperationsTag = "Operations";

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

    public static void MapTeacherSubstitutions(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/teacher-substitutions/active", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetActiveSubstitutionsQuery();
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireAuthorization()
        .WithTags(OperationsTag)
        .WithSummary("Отримання списку поточних активних замін на даний момент часу (Всі ролі)")
        .Produces<IEnumerable<TeacherSubstitutionResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        app.MapGet("/api/teacher-substitutions/by-teacher/{teacherId:guid}", async (
            [FromRoute] Guid teacherId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetSubstitutionsByTeacherIdQuery(teacherId);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireAuthorization()
        .WithTags(OperationsTag)
        .WithSummary("Отримання списку активних замін для конкретного вчителя (Всі ролі)")
        .Produces<IEnumerable<TeacherSubstitutionResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        app.MapGet("/api/teacher-substitutions/by-assignment/{assignmentId:guid}", async (
            [FromRoute] Guid assignmentId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetSubstitutionsByAssignmentIdQuery(assignmentId);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireAuthorization()
        .WithTags(OperationsTag)
        .WithSummary("Отримання списку активних замін для конкретного призначення (Всі ролі)")
        .Produces<IEnumerable<TeacherSubstitutionResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        app.MapGet("/api/teacher-substitutions/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTeacherSubstitutionByIdQuery(id);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireAuthorization()
        .WithTags(OperationsTag)
        .WithSummary("Отримання деталей заміни вчителя за ID (Всі автентифіковані користувачі)")
        .Produces<TeacherSubstitutionResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound);
        
        app.MapPost("/api/teacher-substitutions", async (
            [FromBody] CreateTeacherSubstitutionRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateTeacherSubstitutionCommand(
                request.AssignmentId,
                request.SubstituteTeacherId,
                request.StartDate,
                request.EndDate);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(new { SubstitutionId = result.Value });
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(OperationsTag)
        .WithSummary("Призначення вчителя на заміну (Admin, Director)")
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
.ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPut("/api/teacher-substitutions/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateTeacherSubstitutionRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateTeacherSubstitutionCommand(
                id,
                request.AssignmentId,
                request.SubstituteTeacherId,
                request.StartDate,
                request.EndDate,
                request.RowVersionBase64);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(OperationsTag)
        .WithSummary("Оновлення заміни вчителя (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
.ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapDelete("/api/teacher-substitutions/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] DeleteTeacherSubstitutionRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteTeacherSubstitutionCommand(
                id,
                request.RowVersionBase64);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(OperationsTag)
        .WithSummary("М'яке видалення заміни вчителя (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}