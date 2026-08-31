using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Identity.Role.GetRoles;
using SchoolJournal.Contracts.DTOs.Identity.Roles;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Identity;

internal static class RolesEndpoint
{
    private const string IdentityTag = "Identity.Roles";

    public static void MapRoles(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/roles", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetRolesQuery();
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(IdentityTag)
        .WithSummary("Отримання списку ролей для призначення (Admin, Director)")
        .Produces<IEnumerable<RoleResponse>>(StatusCodes.Status200OK);
    }
}