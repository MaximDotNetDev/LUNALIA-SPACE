using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Infrastructure.GetAuditLogs;
using SchoolJournal.Contracts.DTOs.Infrastructure;
using SchoolJournal.Contracts.DTOs.Infrastructure.AuditLog;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Api.Endpoints.Infrastructure;

internal static class AuditLogsEndpoint
{
    public static void MapAuditLogs(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/audit-logs", async (
            [FromQuery] Guid? userId,
            [FromQuery] DateTimeOffset? fromDate,
            [FromQuery] DateTimeOffset? toDate,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAuditLogsQuery(userId, fromDate, toDate);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return Results.Ok(result);
        })
        .RequireRoles(RoleType.Admin) 
        .Produces<IEnumerable<AuditLogResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithTags("Infrastructure")
        .WithSummary("Отримання логів аудиту (Тільки для Admin)")
        .WithDescription("Повертає список дій користувачів у системі. Можна фільтрувати за UserId та датами.");
    }
}