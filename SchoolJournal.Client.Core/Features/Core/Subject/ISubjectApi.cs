using Refit;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Core.Subjects;

namespace SchoolJournal.Client.Core.Features.Core.Subject;

public interface ISubjectApi
{
    [Get("/api/subjects")]
    public Task<IApiResponse<PagedResponse<SubjectResponse>>> GetActiveSubjectsAsync(
        [Query] int pageNumber,
        [Query] int pageSize,
        [Query] string? searchTerm = null,
        CancellationToken ct = default);

    [Get("/api/subjects/archive")]
    public Task<IApiResponse<PagedResponse<SubjectResponse>>> GetDeletedSubjectsAsync(
        [Query] int pageNumber,
        [Query] int pageSize,
        [Query] string? searchTerm = null,
        CancellationToken ct = default);

    [Get("/api/subjects/{id}")]
    public Task<IApiResponse<SubjectResponse>> GetSubjectByIdAsync(Guid id, CancellationToken ct = default);

    [Post("/api/subjects")]
    public Task<IApiResponse<object>> CreateSubjectAsync([Body] CreateSubjectRequest request, CancellationToken ct = default);

    [Put("/api/subjects/{id}")]
    public Task<IApiResponse> UpdateSubjectAsync(Guid id, [Body] UpdateSubjectRequest request, CancellationToken ct = default);

    [Delete("/api/subjects/{id}")]
    public Task<IApiResponse> DeleteSubjectAsync(Guid id, CancellationToken ct = default);

    [Post("/api/subjects/{id}/restore")]
    public Task<IApiResponse> RestoreSubjectAsync(Guid id, CancellationToken ct = default);

    [Get("/api/teachers/{teacherId}/subjects")]
    public Task<IApiResponse<IReadOnlyCollection<SubjectResponse>>> GetSubjectsByTeacherAsync(Guid teacherId, CancellationToken ct = default);
}