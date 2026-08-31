using Refit;
using SchoolJournal.Contracts.DTOs.Core.StudentSubgroups;

namespace SchoolJournal.Client.Core.Features.Core.StudentSubgroups;

public interface IStudentSubgroupsApi
{
    [Post("/api/student-subgroups")]
    public Task<IApiResponse<object>> AssignStudentToSubgroupAsync([Body] AssignStudentToSubgroupRequest request, CancellationToken ct = default);

    [Delete("/api/student-subgroups/{id}")]
    public Task<IApiResponse> RemoveStudentFromSubgroupAsync(Guid id, CancellationToken ct = default);

    [Put("/api/student-subgroups/{id}/transfer")]
    public Task<IApiResponse> TransferStudentToAnotherSubgroupAsync(Guid id, [Body] TransferStudentToAnotherSubgroupRequest request, CancellationToken ct = default);

    [Post("/api/student-subgroups/{id}/restore")]
    public Task<IApiResponse> RestoreStudentInSubgroupAsync(Guid id, CancellationToken ct = default);

    [Get("/api/subgroups/{subgroupId}/students")]
    public Task<IApiResponse<SubgroupStudentsDetail>> GetStudentsBySubgroupAsync(Guid subgroupId, CancellationToken ct = default);

    [Get("/api/students/{studentId}/subgroups")]
    public Task<IApiResponse<StudentSubgroupsDetail>> GetSubgroupsByStudentAsync(Guid studentId, CancellationToken ct = default);
    [Get("/api/subgroups/{subgroupId}/available-students")]
    public Task<IApiResponse<IEnumerable<AvailableStudentModel>>> GetAvailableStudentsAsync(Guid subgroupId, CancellationToken ct = default);
}