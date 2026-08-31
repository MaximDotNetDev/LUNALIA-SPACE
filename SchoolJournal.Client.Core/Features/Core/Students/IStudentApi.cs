using Refit;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Core.Students;

namespace SchoolJournal.Client.Core.Features.Core.Students;

public interface IStudentApi
{
    [Get("/api/students/{id}")]
    public Task<IApiResponse<StudentResponse>> GetStudentByIdAsync(Guid id, CancellationToken ct = default);

    [Get("/api/students/class/{classId}")]
    public Task<IApiResponse<IEnumerable<StudentLookupResponse>>> GetStudentsByClassIdAsync(Guid classId, CancellationToken ct = default);

    [Get("/api/students/search")]
    public Task<IApiResponse<PagedResponse<StudentSearchResponse>>> SearchStudentsAsync(
        [Query] string? searchTerm,
        [Query] Guid? classId,
        [Query] bool? isActive,
        [Query] int pageNumber,
        [Query] int pageSize,
        CancellationToken ct = default);

    [Post("/api/students")]
    public Task<IApiResponse<object>> CreateStudentAsync([Body] CreateStudentRequest request, CancellationToken ct = default);

    [Put("/api/students/{id}")]
    public Task<IApiResponse> UpdateStudentAsync(Guid id, [Body] UpdateStudentRequest request, CancellationToken ct = default);

    [Delete("/api/students/{id}")]
    public Task<IApiResponse> DeleteStudentAsync(Guid id, [Body] DeleteStudentRequest request, CancellationToken ct = default);

    [Post("/api/students/{id}/transfer")]
    public Task<IApiResponse> TransferStudentAsync(Guid id, [Body] TransferStudentRequest request, CancellationToken ct = default);

    [Post("/api/students/{id}/link-user")]
    public Task<IApiResponse> LinkUserToStudentAsync(Guid id, [Body] LinkUserToStudentRequest request, CancellationToken ct = default);

    [Patch("/api/students/{id}/medical-notes")]
    public Task<IApiResponse> UpdateMedicalNotesAsync(Guid id, [Body] UpdateMedicalNotesRequest request, CancellationToken ct = default);

    [Get("/api/students/{id}/history")]
    public Task<IApiResponse<IEnumerable<StudentHistoryResponse>>> GetStudentHistoryAsync(Guid id, CancellationToken ct = default);

    [Get("/api/students/by-user/{userId}")]
    public Task<IApiResponse<StudentResponse>> GetStudentByUserIdAsync(Guid userId, CancellationToken ct = default);
}