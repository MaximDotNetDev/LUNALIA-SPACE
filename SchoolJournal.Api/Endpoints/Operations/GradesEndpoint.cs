using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Operations.Grade.CreateGrade;
using SchoolJournal.Application.Features.Operations.Grade.DeleteGrade;
using SchoolJournal.Application.Features.Operations.Grade.GetGradeById;
using SchoolJournal.Application.Features.Operations.Grade.GetGradesByLesson;
using SchoolJournal.Application.Features.Operations.Grade.GetGradesByStudent;
using SchoolJournal.Application.Features.Operations.Grade.UpdateGrade;
using SchoolJournal.Contracts.DTOs.Operations.Grades;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Operations;

internal static class GradesEndpoint
{
    private const string Tag = "Grades";

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

    public static void MapGrades(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/grades", async (
            [FromBody] CreateGradeRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateGradeCommand(
                request.LessonId,
                request.StudentId,
                request.GradeValue,
                request.Comment,
                request.GradeTypeId);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(new { GradeId = result.Value });
        })
        .RequireRoles(RoleType.Teacher, RoleType.Admin, RoleType.Director)
        .WithTags(Tag)
        .WithSummary("Виставлення оцінки (Teacher - тільки свої уроки, Admin, Director)")
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden);

        app.MapPut("/api/grades/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateGradeRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateGradeCommand(
                id,
                request.GradeValue,
                request.Comment,
                request.GradeTypeId,
                request.RowVersionBase64);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Teacher, RoleType.Admin, RoleType.Director)
        .WithTags(Tag)
        .WithSummary("Оновлення оцінки з перевіркою версій та власності (Teacher - тільки свої уроки, Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapDelete("/api/grades/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] DeleteGradeRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteGradeCommand(id, request.RowVersionBase64);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Teacher, RoleType.Admin, RoleType.Director)
        .WithTags(Tag)
        .WithSummary("М'яке видалення оцінки з перевіркою версій та власності (Teacher - тільки свої уроки, Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapGet("/api/grades/{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetGradeByIdQuery(id);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireRoles(RoleType.Teacher, RoleType.Admin, RoleType.Director)
        .WithTags(Tag)
        .WithSummary("Отримання деталей оцінки за ID з перевіркою власності (Teacher - тільки свої уроки, Admin, Director)")
        .Produces<GradeResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapGet("/api/lessons/{lessonId:guid}/grades", async (
            [FromRoute] Guid lessonId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetGradesByLessonQuery(lessonId);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireRoles(RoleType.Teacher, RoleType.Admin, RoleType.Director)
        .WithTags(Tag)
        .WithSummary("Отримання списку всіх активних оцінок за конкретний урок (Teacher - тільки свої уроки, Admin, Director)")
        .Produces<IReadOnlyCollection<GradeResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden);

        app.MapGet("/api/students/{studentId:guid}/grades", async (
            [FromRoute] Guid studentId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetGradesByStudentQuery(studentId);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireRoles(RoleType.Student, RoleType.Parent, RoleType.Teacher, RoleType.Admin, RoleType.Director)
        .WithTags(Tag)
        .WithSummary("Отримання оцінок конкретного студента (З урахуванням прав доступу Студент/Батько/Вчитель/Адмін)")
        .Produces<IReadOnlyCollection<GradeResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

    }
}