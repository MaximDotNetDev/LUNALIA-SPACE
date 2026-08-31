using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Core.StudentSubgroups.AssignStudentToSubgroup;
using SchoolJournal.Application.Features.Core.StudentSubgroups.RemoveStudentFromSubgroup;
using SchoolJournal.Application.Features.Core.StudentSubgroups.TransferStudentToAnotherSubgroup;
using SchoolJournal.Application.Features.Core.StudentSubgroups.RestoreStudentInSubgroup;
using SchoolJournal.Application.Features.Core.StudentSubgroups.GetStudentsBySubgroup;
using SchoolJournal.Application.Features.Core.StudentSubgroups.GetSubgroupsByStudent;
using SchoolJournal.Application.Features.Core.StudentSubgroups.GetStudentSubgroupById;
using SchoolJournal.Contracts.DTOs.Core.StudentSubgroups;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Core;

internal static class StudentSubgroupsEndpoint
{
    private const string CoreTag = "Core";

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

    public static void MapStudentSubgroups(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/student-subgroups/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetStudentSubgroupByIdQuery(id);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(CoreTag)
        .WithSummary("Отримання деталей призначення за ID (Admin, Director)")
        .Produces<StudentSubgroupResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPost("/api/student-subgroups", async (
            [FromBody] AssignStudentToSubgroupRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new AssignStudentToSubgroupCommand(
                request.StudentId,
                request.SubgroupId);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(new { StudentSubgroupId = result.Value });
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(CoreTag)
        .WithSummary("Призначення студента у підгрупу (Admin, Director)")
        .Produces<object>(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapDelete("/api/student-subgroups/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RemoveStudentFromSubgroupCommand(id);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
.RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(CoreTag)
        .WithSummary("Видалення студента з підгрупи (Soft Delete) (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPut("/api/student-subgroups/{id:guid}/transfer", async (
            [FromRoute] Guid id,
            [FromBody] TransferStudentToAnotherSubgroupRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new TransferStudentToAnotherSubgroupCommand(id, request.NewSubgroupId);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(CoreTag)
        .WithSummary("Переведення студента в іншу підгрупу (Admin, Director)")
.Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPost("/api/student-subgroups/{id:guid}/restore", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RestoreStudentInSubgroupCommand(id);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(CoreTag)
        .WithSummary("Відновлення видаленого студента у підгрупі (Admin, Director)")
.Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapGet("/api/subgroups/{subgroupId:guid}/students", async (
            [FromRoute] Guid subgroupId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetStudentsBySubgroupQuery(subgroupId);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
        .WithTags("Subgroups")
        .WithSummary("Отримання списку студентів у підгрупі (Admin, Director, Teacher)")
.Produces<SubgroupStudentsDetail>(StatusCodes.Status200OK);

        app.MapGet("/api/students/{studentId:guid}/subgroups", async (
            [FromRoute] Guid studentId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetSubgroupsByStudentQuery(studentId);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
.RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
        .WithTags("Students")
        .WithSummary("Отримання списку підгруп студента (Admin, Director, Teacher)")
        .Produces<StudentSubgroupsDetail>(StatusCodes.Status200OK);

        app.MapGet("/api/subgroups/{subgroupId:guid}/available-students", async (
            [FromRoute] Guid subgroupId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new SchoolJournal.Application.Features.Core.StudentSubgroups.GetAvailableStudents.GetAvailableStudentsQuery(subgroupId);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags("Subgroups")
        .WithSummary("Отримання списку доступних студентів для підгрупи (Admin, Director)")
        .Produces<IEnumerable<AvailableStudentModel>>(StatusCodes.Status200OK);
    }
}