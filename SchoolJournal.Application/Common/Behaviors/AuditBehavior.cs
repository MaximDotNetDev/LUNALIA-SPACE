using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Infrastructure;
using SchoolJournal.Domain.Entities.Infrastructure.IRepositories;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SchoolJournal.Application.Common.Behaviors;

public sealed partial class AuditBehavior<TRequest, TResponse>(
    ICurrentUserService currentUserService,
    IAuditLogRepository auditLogRepository,
    ILogger<AuditBehavior<TRequest, TResponse>> logger,
    IAuditContext auditContext)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Warning,
        Message = "Audit log write skipped for {EntityName}. UserId {UserId} may be deleted, invalid, or uncommitted (FK violation).")]
    private static partial void LogAuditSkippedWarning(ILogger logger, string entityName, Guid userId, Exception ex);

    private static readonly JsonSerializerOptions _auditJsonOptions = new()
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = { MaskSensitiveProperties }
        },
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,

        Converters = { new ByteCollectionToBase64Converter() }
    };

    private sealed class ByteCollectionToBase64Converter : JsonConverter<IReadOnlyCollection<byte>>
    {
        public override IReadOnlyCollection<byte>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotSupportedException();

        public override void Write(Utf8JsonWriter writer, IReadOnlyCollection<byte> value, JsonSerializerOptions options)
                    => writer.WriteStringValue($"[Binary data: {value.Count} bytes]");
    }

    private static void MaskSensitiveProperties(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
        {
            return;
        }

        var sensitiveProperties = typeInfo.Properties.Where(p =>
                    p.Name.Contains("Password", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("Token", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("Secret", StringComparison.OrdinalIgnoreCase));

        foreach (var property in sensitiveProperties)
        {
            property.Get = _ => "***MASKED***";
        }
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var requestName = typeof(TRequest).Name;
        if (!requestName.EndsWith("Command", StringComparison.OrdinalIgnoreCase))
        {
            // FIXED: Forwarding cancellationToken to next
            return await next(cancellationToken).ConfigureAwait(false);
        }

        var sw = System.Diagnostics.Stopwatch.StartNew();

        // Best Practice: Pass the CancellationToken down into the local function rather than capturing it implicitly
        async Task RecordAuditLogAsync(string actionStatus, string? errorMsg, CancellationToken ct)
        {
            var userId = currentUserService.GetUserId();
            Guid? auditUserId = userId == Guid.Empty ? null : userId;

            var entityRefProperty = request.GetType().GetProperties()
                .FirstOrDefault(p => p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && p.Name != "UserId");
            var entityRefValue = entityRefProperty?.GetValue(request)?.ToString() ?? "System";

            var oldState = auditContext.GetOldState();

            var auditLog = new AuditLog
            {
                UserId = auditUserId,
                EntityName = requestName.Replace("Command", string.Empty, StringComparison.OrdinalIgnoreCase),
                EntityRef = entityRefValue,
                Action = actionStatus,
                OldValue = oldState is null ? null : JsonSerializer.Serialize(oldState, _auditJsonOptions),
                NewValue = JsonSerializer.Serialize(new
                {
                    Payload = request,
                    State = auditContext.GetNewState(),
                    Metadata = new
                    {
                        DurationMs = sw.ElapsedMilliseconds,
                        Error = errorMsg
                    }
                }, _auditJsonOptions),
                OccurredAtUtc = DateTimeOffset.UtcNow,
                ClientIp = currentUserService.GetClientIp(),
                CreatedAt = DateTimeOffset.UtcNow
            };

            try
            {
                await auditLogRepository.AddAsync(auditLog, ct).ConfigureAwait(false);
            }
            catch (System.Data.Common.DbException ex)
            {
                LogAuditSkippedWarning(logger, auditLog.EntityName, userId, ex);
            }
        }

        try
        {
            // FIXED: Forwarding cancellationToken to next
            var response = await next(cancellationToken).ConfigureAwait(false);
            sw.Stop();

            var actionStatus = "Execute";
            string? errorMsg = null;

            if (response is IErrorOr { IsError: true } errorOr)
            {
                actionStatus = "Failed";
                errorMsg = string.Join("; ", errorOr.Errors?.Select(e => e.Description) ?? []);
            }

            await RecordAuditLogAsync(actionStatus, errorMsg, cancellationToken).ConfigureAwait(false);

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            await RecordAuditLogAsync("Failed", ex.Message, cancellationToken).ConfigureAwait(false);
            throw;
        }
    }
}