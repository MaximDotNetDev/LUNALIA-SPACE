using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Infrastructure.SystemSettings.GetSystemSettings;
using SchoolJournal.Application.Features.Infrastructure.SystemSettings.UpdateSystemSettings;
using SchoolJournal.Contracts.DTOs.Infrastructure.SystemSettings;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Infrastructure;

internal static class SystemSettingsEndpoint
{
    private const string InfrastructureTag = "Infrastructure";

    private static IResult HandleError(ErrorOr.Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorOr.ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorOr.ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorOr.ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorOr.ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(statusCode: statusCode, title: error.Description);
    }

    public static void MapSystemSettings(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/system-settings", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetSystemSettingsQuery();
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.Ok(result.Value);
        })
        .RequireAuthorization()
        .WithTags(InfrastructureTag)
        .WithSummary("Отримання системних налаштувань (Всі авторизовані користувачі)")
        .Produces<SystemSettingsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPut("/api/system-settings", async (
            [FromBody] UpdateSystemSettingsRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateSystemSettingsCommand(
                request.SchoolName,
                request.AcademicYear,
                request.PrincipalName,
                request.RowVersionBase64);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsError
                ? HandleError(result.FirstError)
                : Results.NoContent();
        })
        .RequireRoles(RoleType.Admin, RoleType.Director)
        .WithTags(InfrastructureTag)
        .WithSummary("Оновлення системних налаштувань (Admin, Director)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}