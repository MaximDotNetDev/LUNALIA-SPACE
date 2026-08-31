using Refit;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.Semesters;

namespace SchoolJournal.Client.Core.Features.Reference.Semester;

public interface ISemesterApi
{
    [Get("/api/semesters")]
    public Task<IApiResponse<PagedResponse<SemesterResponse>>> GetActiveSemestersAsync(
        [Query] int pageNumber,
        [Query] int pageSize,
        CancellationToken ct = default);

    [Get("/api/semesters/archive")]
    public Task<IApiResponse<PagedResponse<SemesterResponse>>> GetDeletedSemestersAsync(
        [Query] int pageNumber,
        [Query] int pageSize,
        CancellationToken ct = default);

    [Get("/api/semesters/{id}")]
    public Task<IApiResponse<SemesterResponse>> GetSemesterByIdAsync(Guid id, CancellationToken ct = default);

    [Post("/api/semesters")]
    public Task<IApiResponse<object>> CreateSemesterAsync([Body] CreateSemesterRequest request, CancellationToken ct = default);

    [Put("/api/semesters/{id}")]
    public Task<IApiResponse> UpdateSemesterAsync(Guid id, [Body] UpdateSemesterRequest request, CancellationToken ct = default);

    [Delete("/api/semesters/{id}")]
    public Task<IApiResponse> DeleteSemesterAsync(Guid id, [Body] DeleteSemesterRequest request, CancellationToken ct = default);

    [Post("/api/semesters/{id}/restore")]
    public Task<IApiResponse> RestoreSemesterAsync(Guid id, [Body] RestoreSemesterRequest request, CancellationToken ct = default);
}