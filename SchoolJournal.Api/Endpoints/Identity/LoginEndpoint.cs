using MediatR;
using SchoolJournal.Api.Common.Mapping;
using SchoolJournal.Application.Features.Identity.Login;
using SchoolJournal.Contracts.DTOs.Identity.Login;

namespace SchoolJournal.Api.Endpoints.Identity;

internal static class LoginEndpoint
{
    public static void MapLogin(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/login",
                async (
                    [Microsoft.AspNetCore.Mvc.FromBody] LoginRequest request,
                    ISender mediator,
                    CancellationToken ct) =>
                    {
                var command = new LoginCommand(
                    request.Login,
                    request.Password,
                    request.DeviceIdentifier);

                        var result = await mediator.Send(command, ct).ConfigureAwait(false);

                        return result.Match(
                            tokenResponse =>
                            {
                                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                                var jwt = handler.ReadJwtToken(tokenResponse.AccessToken);
                                var roleClaim = jwt.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

                                var role = SchoolJournal.Contracts.Enums.Identity.RoleType.None;
                                if (!string.IsNullOrWhiteSpace(roleClaim) &&
                                    Enum.TryParse<SchoolJournal.Contracts.Enums.Identity.RoleType>(roleClaim, out var parsedRole))
                                {
                                    role = parsedRole;
                                }

                                var loginResponse = new LoginResponse(
                                    tokenResponse.AccessToken,
                                    tokenResponse.RefreshToken,
                                    tokenResponse.ExpiresInSeconds,
                                    role);

                                return Results.Ok(loginResponse);
                            },
                            errors => errors.ToProblem());
                    })
        .WithName("Login")
        .RequireRateLimiting("login_limiter")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status429TooManyRequests);
    }
}