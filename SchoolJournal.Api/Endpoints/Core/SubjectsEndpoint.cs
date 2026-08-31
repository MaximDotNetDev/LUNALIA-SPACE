using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Core.Subject.CreateSubject;
using SchoolJournal.Application.Features.Core.Subject.DeleteSubject;
using SchoolJournal.Application.Features.Core.Subject.GetSubjectById;
using SchoolJournal.Application.Features.Core.Subject.GetSubjects;
using SchoolJournal.Application.Features.Core.Subject.RestoreSubject;
using SchoolJournal.Application.Features.Core.Subject.UpdateSubject;
using SchoolJournal.Application.Features.Core.Subject.GetDeletedSubjects;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Core.Subjects;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Core;

internal static class SubjectsEndpoint
{
    private const string CoreTag = "Core.Subjects";

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

    private static PageRequest CreatePageRequest(int pageNumber, int pageSize)
    {
        return new PageRequest(pageNumber > 0 ? pageNumber : 1, pageSize > 0 ? pageSize : 10);
    }

    public static void MapSubjects(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/subjects", GetSubjectsAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("Отримання списку активних предметів з пагінацією (Admin, Director)")
            .Produces<PagedResponse<SubjectResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        app.MapGet("/api/subjects/archive", GetDeletedSubjectsAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("Отримання архіву видалених предметів (Admin, Director)")
            .Produces<PagedResponse<SubjectResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        app.MapGet("/api/teachers/{teacherId:guid}/subjects", GetSubjectsByTeacherIdAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
            .WithTags(CoreTag)
            .WithSummary("Отримання списку предметів конкретного вчителя")
            .Produces<IEnumerable<SubjectResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        app.MapGet("/api/subjects/{id:guid}", GetSubjectByIdAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("Отримання деталей предмета за ID (Admin, Director)")
            .Produces<SubjectResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPost("/api/subjects", CreateSubjectAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("Створення нового предмета (Admin, Director)")
            .Produces<object>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPut("/api/subjects/{id:guid}", UpdateSubjectAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("Оновлення предмета (Admin, Director)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapDelete("/api/subjects/{id:guid}", DeleteSubjectAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("М'яке видалення предмета (Admin, Director)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPost("/api/subjects/{id:guid}/restore", RestoreSubjectAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("Відновлення м'яко видаленого предмета (Admin, Director)")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> GetSubjectsAsync(
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        [FromQuery] string? searchTerm,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetSubjectsQuery(CreatePageRequest(pageNumber, pageSize), searchTerm);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetDeletedSubjectsAsync(
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        [FromQuery] string? searchTerm,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetDeletedSubjectsQuery(CreatePageRequest(pageNumber, pageSize), searchTerm);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetSubjectsByTeacherIdAsync(
        [FromRoute] Guid teacherId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new SchoolJournal.Application.Features.Core.Subject.GetSubjectsByTeacherId.GetSubjectsByTeacherIdQuery(teacherId);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
        return result.IsError ? HandleError(result.FirstError) : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetSubjectByIdAsync(
        [FromRoute] Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetSubjectByIdQuery(id);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
        return result.IsError ? HandleError(result.FirstError) : Results.Ok(result.Value);
    }

    private static async Task<IResult> CreateSubjectAsync(
        [FromBody] CreateSubjectRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateSubjectCommand(request.SubjectName);
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
        return result.IsError ? HandleError(result.FirstError) : Results.Ok(new { SubjectId = result.Value });
    }

    private static async Task<IResult> UpdateSubjectAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateSubjectRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSubjectCommand(id, request.SubjectName);
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
        return result.IsError ? HandleError(result.FirstError) : Results.NoContent();
    }

    private static async Task<IResult> DeleteSubjectAsync(
        [FromRoute] Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new DeleteSubjectCommand(id);
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
        return result.IsError ? HandleError(result.FirstError) : Results.NoContent();
    }

    private static async Task<IResult> RestoreSubjectAsync(
        [FromRoute] Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new RestoreSubjectCommand(id);
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
        return result.IsError ? HandleError(result.FirstError) : Results.Ok();
    }
}