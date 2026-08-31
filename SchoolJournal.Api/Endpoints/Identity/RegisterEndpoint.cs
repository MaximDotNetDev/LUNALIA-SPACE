using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Api.Common.Mapping;
using SchoolJournal.Application.Features.Identity.Register;
using SchoolJournal.Contracts.DTOs.Identity.Register;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Identity;

internal static class RegisterEndpoint
{
    public static void MapRegister(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/register", async (
                    [FromBody] RegisterRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
        {
            try
            {
                var command = new RegisterCommand(request.Login, request.Password, request.Role);
                var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

                return result.Match(
                    userId => Results.Ok(new RegisterResponse(userId)),
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
        .WithName("Register")
        .WithTags("Identity")
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .Produces<RegisterResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}