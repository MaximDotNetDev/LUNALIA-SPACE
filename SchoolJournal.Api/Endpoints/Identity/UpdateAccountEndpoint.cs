using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Api.Common.Mapping;
using SchoolJournal.Application.Features.Identity.UpdateAccount;
using SchoolJournal.Contracts.DTOs.Identity.UpdateAccount;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Identity;

internal static class UpdateAccountEndpoint
{
    public static void MapUpdateAccount(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/identity/users/{userId:guid}", async (
            [FromRoute] Guid userId,
            [FromBody] UpdateAccountRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var command = new UpdateAccountCommand(userId, request.Login, request.NewPassword);
                var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

                return result.Match(
                    _ => Results.NoContent(),
                    errors => errors.ToProblem()
                );
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: ex.Message);
            }
        })
        .WithName("UpdateAccount")
        .WithTags("Identity")
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}