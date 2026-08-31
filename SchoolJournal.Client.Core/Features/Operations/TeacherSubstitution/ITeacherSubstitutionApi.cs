using Refit;
using SchoolJournal.Contracts.DTOs.Operations.TeacherSubstitutions;

namespace SchoolJournal.Client.Core.Features.Operations.TeacherSubstitution;

public interface ITeacherSubstitutionApi
{
    [Post("/api/teacher-substitutions")]
    public Task<IApiResponse<object>> CreateSubstitutionAsync([Body] CreateTeacherSubstitutionRequest request, CancellationToken ct = default);

    [Put("/api/teacher-substitutions/{id}")]
    public Task<IApiResponse> UpdateSubstitutionAsync(Guid id, [Body] UpdateTeacherSubstitutionRequest request, CancellationToken ct = default);

    [Delete("/api/teacher-substitutions/{id}")]
    public Task<IApiResponse> DeleteSubstitutionAsync(Guid id, [Body] DeleteTeacherSubstitutionRequest request, CancellationToken ct = default);

    [Get("/api/teacher-substitutions/{id}")]
    public Task<IApiResponse<TeacherSubstitutionResponse>> GetSubstitutionByIdAsync(Guid id, CancellationToken ct = default);

    [Get("/api/teacher-substitutions/by-assignment/{assignmentId}")]
    public Task<IApiResponse<IEnumerable<TeacherSubstitutionResponse>>> GetSubstitutionsByAssignmentIdAsync(Guid assignmentId, CancellationToken ct = default);

    [Get("/api/teacher-substitutions/by-teacher/{teacherId}")]
    public Task<IApiResponse<IEnumerable<TeacherSubstitutionResponse>>> GetSubstitutionsByTeacherIdAsync(Guid teacherId, CancellationToken ct = default);

    [Get("/api/teacher-substitutions/active")]
    public Task<IApiResponse<IEnumerable<TeacherSubstitutionResponse>>> GetActiveSubstitutionsAsync(CancellationToken ct = default);
}