using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Common.Extensions;

internal static class AuthorizationExtensions
{
    public static RouteHandlerBuilder RequireRoles(
        this RouteHandlerBuilder builder,
        params RoleType[] roles)
    {
        IEnumerable<string> roleNames = roles.Select(role => role.ToString());
        var authorizeAttribute = new AuthorizeAttribute { Roles = string.Join(',', roleNames) };

        return builder.RequireAuthorization(authorizeAttribute);
    }
}