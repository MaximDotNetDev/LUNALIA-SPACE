using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Core.Subgroup.CreateSubgroup;
using SchoolJournal.Application.Features.Core.Subgroup.DeleteSubgroup;
using SchoolJournal.Application.Features.Core.Subgroup.GetSubgroupById;
using SchoolJournal.Application.Features.Core.Subgroup.GetSubgroupsByClass;
using SchoolJournal.Application.Features.Core.Subgroup.GetSubgroupsBySubject;
using SchoolJournal.Application.Features.Core.Subgroup.GetSubgroupsList;
using SchoolJournal.Application.Features.Core.Subgroup.RestoreSubgroup;
using SchoolJournal.Application.Features.Core.Subgroup.UpdateSubgroup;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Core.Subgroups;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Core;

internal static class SubgroupsEndpoint
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

    public static void MapSubgroups(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/subgroups/{id:guid}", GetSubgroupById)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("Отримання деталей підгрупи за ID (Admin, Director)")
            .Produces<SubgroupResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPost("/api/subgroups", CreateSubgroup)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("Створення нової підгрупи (Admin, Director)")
            .Produces<object>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPut("/api/subgroups/{id:guid}", UpdateSubgroup)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("Оновлення даних підгрупи (Admin, Director)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapDelete("/api/subgroups/{id:guid}", DeleteSubgroup)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("М'яке видалення підгрупи (Admin, Director)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPost("/api/subgroups/{id:guid}/restore", RestoreSubgroup)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("Відновлення м'яко видаленої підгрупи (Admin, Director)")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapGet("/api/classes/{classId:guid}/subgroups", GetSubgroupsByClass)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("Отримання списку підгруп конкретного класу (Admin, Director)")
            .Produces<IReadOnlyCollection<SubgroupResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapGet("/api/classes/{classId:guid}/subjects/{subjectId:guid}/subgroups", GetSubgroupsBySubject)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("Отримання підгруп за предметом та класом (Admin, Director)")
            .Produces<IReadOnlyCollection<SubgroupResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapGet("/api/subgroups", GetSubgroupsList)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("Отримання пагінованого списку всіх підгруп (Admin, Director)")
            .Produces<PagedResponse<SubgroupResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetSubgroupById(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetSubgroupByIdQuery(id), ct).ConfigureAwait(false);
        return result.IsError ? HandleError(result.FirstError) : Results.Ok(result.Value);
    }

    private static async Task<IResult> CreateSubgroup([FromBody] CreateSubgroupRequest request, ISender sender, CancellationToken ct)
    {
        var command = new CreateSubgroupCommand(request.ClassId, request.SubjectId, request.SubgroupName);
        var result = await sender.Send(command, ct).ConfigureAwait(false);
        return result.IsError ? HandleError(result.FirstError) : Results.Ok(new { SubgroupId = result.Value });
    }

    private static async Task<IResult> UpdateSubgroup(Guid id, [FromBody] UpdateSubgroupRequest request, ISender sender, CancellationToken ct)
    {
        var command = new UpdateSubgroupCommand(id, request.SubgroupName, request.IsActive, request.RowVersionBase64);
        var result = await sender.Send(command, ct).ConfigureAwait(false);
        return result.IsError ? HandleError(result.FirstError) : Results.NoContent();
    }

    private static async Task<IResult> DeleteSubgroup(Guid id, [FromBody] DeleteSubgroupRequest request, ISender sender, CancellationToken ct)
    {
        var command = new DeleteSubgroupCommand(id, request.RowVersionBase64);
        var result = await sender.Send(command, ct).ConfigureAwait(false);
        return result.IsError ? HandleError(result.FirstError) : Results.NoContent();
    }

    private static async Task<IResult> RestoreSubgroup(Guid id, [FromBody] RestoreSubgroupRequest request, ISender sender, CancellationToken ct)
    {
        var command = new RestoreSubgroupCommand(id, request.RowVersionBase64);
        var result = await sender.Send(command, ct).ConfigureAwait(false);
        return result.IsError ? HandleError(result.FirstError) : Results.Ok();
    }

    private static async Task<IResult> GetSubgroupsByClass(Guid classId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetSubgroupsByClassQuery(classId), ct).ConfigureAwait(false);
        return result.IsError ? HandleError(result.FirstError) : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetSubgroupsBySubject(Guid classId, Guid subjectId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetSubgroupsBySubjectQuery(classId, subjectId), ct).ConfigureAwait(false);
        return result.IsError ? HandleError(result.FirstError) : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetSubgroupsList([FromQuery] int pageNumber, [FromQuery] int pageSize, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetSubgroupsListQuery(new PageRequest(pageNumber, pageSize)), ct).ConfigureAwait(false);
        return result.IsError ? HandleError(result.FirstError) : Results.Ok(result.Value);
    }
}