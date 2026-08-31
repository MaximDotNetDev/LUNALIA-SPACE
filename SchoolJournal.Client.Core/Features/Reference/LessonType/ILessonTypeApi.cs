using Refit;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.LessonTypes;

namespace SchoolJournal.Client.Core.Features.Reference.LessonType;

public interface ILessonTypeApi
{
    [Get("/api/lessontypes")]
    public Task<IApiResponse<PagedResponse<LessonTypeResponse>>> GetActiveLessonTypesAsync(
        [Query] int pageNumber,
        [Query] int pageSize,
        CancellationToken ct = default);

    [Get("/api/lessontypes/archive")]
    public Task<IApiResponse<PagedResponse<LessonTypeResponse>>> GetDeletedLessonTypesAsync(
        [Query] int pageNumber,
        [Query] int pageSize,
        CancellationToken ct = default);

    [Get("/api/lessontypes/{id}")]
    public Task<IApiResponse<LessonTypeResponse>> GetLessonTypeByIdAsync(Guid id, CancellationToken ct = default);

    [Post("/api/lessontypes")]
    public Task<IApiResponse<object>> CreateLessonTypeAsync([Body] CreateLessonTypeRequest request, CancellationToken ct = default);

    [Put("/api/lessontypes/{id}")]
    public Task<IApiResponse> UpdateLessonTypeAsync(Guid id, [Body] UpdateLessonTypeRequest request, CancellationToken ct = default);

    [Delete("/api/lessontypes/{id}")]
    public Task<IApiResponse> DeleteLessonTypeAsync(Guid id, CancellationToken ct = default);

    [Post("/api/lessontypes/{id}/restore")]
    public Task<IApiResponse> RestoreLessonTypeAsync(Guid id, CancellationToken ct = default);
}