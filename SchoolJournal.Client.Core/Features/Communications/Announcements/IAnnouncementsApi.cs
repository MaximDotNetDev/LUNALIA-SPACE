using Refit;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Communications.Announcements;

namespace SchoolJournal.Client.Core.Features.Communications.Announcements;

public interface IAnnouncementsApi
{
    [Post("/api/announcements")]
    public Task<IApiResponse<object>> CreateAnnouncementAsync(
        [Body] CreateAnnouncementRequest request,
        CancellationToken cancellationToken = default);

    [Put("/api/announcements/{id}")]
    public Task<IApiResponse> UpdateAnnouncementAsync(
        Guid id,
        [Body] UpdateAnnouncementRequest request,
        CancellationToken cancellationToken = default);

    [Patch("/api/announcements/{id}/toggle")]
    public Task<IApiResponse> ToggleStatusAsync(
        Guid id,
        [Body] ToggleAnnouncementStatusRequest request,
        CancellationToken cancellationToken = default);

    [Delete("/api/announcements/{id}")]
    public Task<IApiResponse> DeleteAnnouncementAsync(
        Guid id,
        [Body] DeleteAnnouncementRequest request,
        CancellationToken cancellationToken = default);

    [Get("/api/announcements")]
    public Task<IApiResponse<PagedResponse<AnnouncementResponse>>> GetActiveAnnouncementsAsync(
        [Query] int pageNumber,
        [Query] int pageSize,
        CancellationToken cancellationToken = default);

    [Get("/api/announcements/{id}")]
    public Task<IApiResponse<AnnouncementResponse>> GetAnnouncementByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    [Get("/api/announcements/admin")]
    public Task<IApiResponse<PagedResponse<AnnouncementResponse>>> GetAdminAnnouncementsAsync(
        [Query] int pageNumber,
        [Query] int pageSize,
        [Query] string? search = null,
        [Query] bool? isActive = null,
        [Query] Guid? authorId = null,
        CancellationToken cancellationToken = default);
}