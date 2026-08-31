using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Core.Parent.CreateParent;
using SchoolJournal.Contracts.DTOs.Core.Parents;
using SchoolJournal.Domain.Enums.Identity;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Application.Features.Core.Parent.UpdateParent;
using SchoolJournal.Application.Features.Core.Parent.DeleteParent;
using SchoolJournal.Application.Features.Core.Parent.LinkParentToUser;
using SchoolJournal.Application.Features.Core.Parent.ToggleParentStatus;
using SchoolJournal.Application.Features.Core.Parent.GetParentById;
using SchoolJournal.Application.Features.Core.Parent.GetParentsPaged;
using SchoolJournal.Application.Features.Core.Parent.GetParentByUserId;

namespace SchoolJournal.Api.Endpoints.Core;

internal static class ParentsEndpoint
{
    private const string CoreTag = "Parents";

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

    public static void MapParents(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/parents/my-profile", GetMyProfileAsync)
            .RequireAuthorization()
            .WithTags(CoreTag)
            .WithSummary("Отримання профілю поточного авторизованого батька (Self)")
            .Produces<ParentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapGet("/api/parents", GetParentsPagedAsync)
            .RequireAuthorization()
            .WithTags(CoreTag)
            .WithSummary("Отримання списку активних батьків з пагінацією (Всі ролі)")
            .Produces<PagedResponse<ParentResponse>>(StatusCodes.Status200OK);

        app.MapGet("/api/parents/{id:guid}", GetParentByIdAsync)
            .RequireAuthorization()
            .WithTags(CoreTag)
            .WithSummary("Отримання деталей профілю батьків за ID (Всі авторизовані)")
            .Produces<ParentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPost("/api/parents/{id:guid}/toggle-status", ToggleStatusAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("Зміна статусу профілю батька (активний/неактивний) (Admin, Director)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPost("/api/parents/{id:guid}/link-user", LinkUserAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("Прив'язка профілю батька до облікового запису (Admin, Director)")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapDelete("/api/parents/{id:guid}", DeleteParentAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("М'яке видалення профілю батьків (Admin, Director)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPut("/api/parents/{id:guid}", UpdateParentAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("Оновлення профілю батьків (Admin, Director)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPost("/api/parents", CreateParentAsync)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithTags(CoreTag)
            .WithSummary("Створення профілю батьків (Admin, Director)")
            .Produces<object>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        static async Task<IResult> GetMyProfileAsync(ICurrentUserService currentUserService, ISender sender, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetUserId();
            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var query = new GetParentByUserIdQuery(userId);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError ? HandleError(result.FirstError) : Results.Ok(result.Value);
        }

        static async Task<IResult> GetParentsPagedAsync([FromQuery] int pageNumber, [FromQuery] int pageSize, ISender sender, CancellationToken cancellationToken)
        {
            var query = new GetParentsPagedQuery(new PageRequest(pageNumber, pageSize));
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return Results.Ok(result);
        }

        static async Task<IResult> GetParentByIdAsync([FromRoute] Guid id, ISender sender, CancellationToken cancellationToken)
        {
            var query = new GetParentByIdQuery(id);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError ? HandleError(result.FirstError) : Results.Ok(result.Value);
        }

        static async Task<IResult> ToggleStatusAsync([FromRoute] Guid id, [FromBody] ToggleParentStatusRequest request, ISender sender, CancellationToken cancellationToken)
        {
            var command = new ToggleParentStatusCommand(id, request.RowVersionBase64);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError ? HandleError(result.FirstError) : Results.NoContent();
        }

        static async Task<IResult> LinkUserAsync([FromRoute] Guid id, [FromBody] LinkParentToUserRequest request, ISender sender, CancellationToken cancellationToken)
        {
            var command = new LinkParentToUserCommand(id, request.UserId, request.RowVersionBase64);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError ? HandleError(result.FirstError) : Results.Ok();
        }

        static async Task<IResult> DeleteParentAsync([FromRoute] Guid id, [FromBody] DeleteParentRequest request, ISender sender, CancellationToken cancellationToken)
        {
            var command = new DeleteParentCommand(id, request.RowVersionBase64);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError ? HandleError(result.FirstError) : Results.NoContent();
        }

        static async Task<IResult> UpdateParentAsync([FromRoute] Guid id, [FromBody] UpdateParentRequest request, ISender sender, CancellationToken cancellationToken)
        {
            var command = new UpdateParentCommand(id, request.LastName, request.FirstName, request.MiddleName, request.Phone, request.RowVersionBase64);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError ? HandleError(result.FirstError) : Results.NoContent();
        }

        static async Task<IResult> CreateParentAsync([FromBody] CreateParentRequest request, ISender sender, CancellationToken cancellationToken)
        {
            var command = new CreateParentCommand(request.LastName, request.FirstName, request.MiddleName, request.Phone);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError ? HandleError(result.FirstError) : Results.Ok(new { ParentId = result.Value });
        }
    }
}