using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Mapping;
using SchoolJournal.Application.Features.Identity.Logout;
using SchoolJournal.Contracts.DTOs.Identity.Logout;

namespace SchoolJournal.Api.Endpoints.Identity;

internal static class LogoutEndpoint
{
    public static void MapLogoutEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/logout", async (
            [FromBody] LogoutRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new LogoutCommand(request.RefreshToken);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.Match(
                _ => Results.NoContent(),
                errors => errors.ToProblem()
            );
        })
        .WithName("Logout")
        .WithTags("Identity")
        .RequireAuthorization()
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}