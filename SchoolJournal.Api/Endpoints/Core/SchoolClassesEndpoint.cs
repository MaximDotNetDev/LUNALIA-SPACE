using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Application.Features.Core.SchoolClasses.ActivateSchoolClass;
using SchoolJournal.Application.Features.Core.SchoolClasses.AssignHomeroomTeacher;
using SchoolJournal.Application.Features.Core.SchoolClasses.CreateSchoolClass;
using SchoolJournal.Application.Features.Core.SchoolClasses.DeactivateSchoolClass;
using SchoolJournal.Application.Features.Core.SchoolClasses.DeleteSchoolClass;
using SchoolJournal.Application.Features.Core.SchoolClasses.GetActiveSchoolClasses;
using SchoolJournal.Application.Features.Core.SchoolClasses.GetClassesByTeacherId;
using SchoolJournal.Application.Features.Core.SchoolClasses.GetSchoolClassById;
using SchoolJournal.Application.Features.Core.SchoolClasses.UpdateSchoolClass;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Core.SchoolClasses;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Core;

internal static class SchoolClassesEndpoint
{
    private const string CoreTag = "Core";

    public static void MapSchoolClasses(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/classes", CreateSchoolClassAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("Створення нового класу (Admin, Director)")
            .Produces<object>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPut("/api/classes/{id:guid}", UpdateSchoolClassAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("Оновлення класу з перевіркою версій (Admin, Director)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPatch("/api/classes/{id:guid}/teacher", AssignHomeroomTeacherAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("Зміна класного керівника (Admin, Director)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPost("/api/classes/{id:guid}/activate", ActivateSchoolClassAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("Активація класу (Admin, Director)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPost("/api/classes/{id:guid}/deactivate", DeactivateSchoolClassAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("Деактивація класу (Admin, Director)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapDelete("/api/classes/{id:guid}", DeleteSchoolClassAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("М'яке видалення класу (Admin, Director)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapGet("/api/classes", GetActiveSchoolClassesAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
            .WithTags(CoreTag)
            .WithSummary("Отримання списку активних класів з пагінацією (Admin, Director, Teacher)")
            .Produces<PagedResponse<SchoolClassItemResponse>>(StatusCodes.Status200OK);

        app.MapGet("/api/classes/{id:guid}", GetSchoolClassByIdAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("Отримання деталей класу за ID (Admin, Director)")
            .Produces<SchoolClassResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapGet("/api/my/classes", GetMyClassesAsync)
            .RequireRoles(RoleType.Teacher)
            .WithTags(CoreTag)
            .WithSummary("Отримання власних класів вчителя (Тільки вчитель)")
            .Produces<IEnumerable<SchoolClassItemResponse>>(StatusCodes.Status200OK);

        app.MapGet("/api/teachers/{teacherId:guid}/classes", GetClassesByTeacherIdAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("Отримання класів конкретного вчителя (Admin, Director)")
            .Produces<IEnumerable<SchoolClassItemResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CreateSchoolClassAsync([FromBody] CreateSchoolClassRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var command = new CreateSchoolClassCommand(request.ClassName, request.GradeLevel, request.AcademicYear, request.HomeroomTeacherId);
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

        return result.IsError
            ? Results.Problem(statusCode: StatusCodes.Status409Conflict, title: result.FirstError.Description)
            : Results.Ok(new { ClassId = result.Value });
    }

    private static async Task<IResult> UpdateSchoolClassAsync([FromRoute] Guid id, [FromBody] UpdateSchoolClassRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var command = new UpdateSchoolClassCommand(id, request.ClassName, request.GradeLevel, request.AcademicYear, request.HomeroomTeacherId, request.RowVersionBase64);
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

        return HandleErrorOrSuccess(result);
    }

    private static async Task<IResult> AssignHomeroomTeacherAsync([FromRoute] Guid id, [FromBody] AssignHomeroomTeacherRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var command = new AssignHomeroomTeacherCommand(id, request.NewHomeroomTeacherId, request.RowVersionBase64);
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

        return HandleErrorOrSuccess(result);
    }

    private static async Task<IResult> ActivateSchoolClassAsync([FromRoute] Guid id, [FromBody] ChangeSchoolClassStatusRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var command = new ActivateSchoolClassCommand(id, request.RowVersionBase64);
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

        return HandleErrorOrSuccess(result);
    }

    private static async Task<IResult> DeactivateSchoolClassAsync([FromRoute] Guid id, [FromBody] ChangeSchoolClassStatusRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var command = new DeactivateSchoolClassCommand(id, request.RowVersionBase64);
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

        return HandleErrorOrSuccess(result);
    }

    private static async Task<IResult> DeleteSchoolClassAsync([FromRoute] Guid id, [FromBody] DeleteSchoolClassRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var command = new DeleteSchoolClassCommand(id, request.RowVersionBase64);
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

        return HandleErrorOrSuccess(result);
    }

    private static async Task<IResult> GetActiveSchoolClassesAsync([FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] string? academicYear, ISender sender, CancellationToken cancellationToken)
    {
        var query = new GetActiveSchoolClassesQuery(new PageRequest(pageNumber, pageSize), academicYear);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetSchoolClassByIdAsync([FromRoute] Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var query = new GetSchoolClassByIdQuery(id);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

        if (!result.IsError)
        {
            return Results.Ok(result.Value);
        }

        return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: result.FirstError.Description);
    }

    private static async Task<IResult> GetMyClassesAsync(ISender sender, ICurrentUserService currentUserService, CancellationToken cancellationToken)
    {
        var teacherId = currentUserService.GetUserId();
        var query = new GetClassesByTeacherIdQuery(teacherId);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetClassesByTeacherIdAsync([FromRoute] Guid teacherId, ISender sender, CancellationToken cancellationToken)
    {
        var query = new GetClassesByTeacherIdQuery(teacherId);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

        return Results.Ok(result);
    }

    private static IResult HandleErrorOrSuccess(ErrorOr.ErrorOr<ErrorOr.Success> result)
    {
        if (!result.IsError)
        {
            return Results.NoContent();
        }

        var statusCode = result.FirstError.Type == ErrorOr.ErrorType.NotFound
            ? StatusCodes.Status404NotFound
            : StatusCodes.Status409Conflict;

        return Results.Problem(statusCode: statusCode, title: result.FirstError.Description);
    }
}