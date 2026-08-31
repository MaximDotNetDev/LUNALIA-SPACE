using Refit;
using SchoolJournal.Contracts.DTOs.Infrastructure.SystemSettings;

namespace SchoolJournal.Client.Core.Features.Infrastructure.SystemSettings;

public interface ISystemSettingsApi
{
    [Get("/api/system-settings")]
    public Task<IApiResponse<SystemSettingsResponse>> GetSettingsAsync(CancellationToken ct = default);

    [Put("/api/system-settings")]
    public Task<IApiResponse> UpdateSettingsAsync([Body] UpdateSystemSettingsRequest request, CancellationToken ct = default);
}