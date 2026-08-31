using SchoolJournal.Contracts.DTOs.Operations.TeachingAssignments;

namespace SchoolJournal.Application.Features.Operations.TeachingAssignments;

public interface ITeachingAssignmentQueries
{
    public Task<TeachingAssignmentResponse?> GetByIdAsync(Guid assignmentId, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<TeachingAssignmentResponse> Items, int TotalCount)> GetPagedByTeacherIdAsync(Guid teacherId, int skip, int take, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<TeachingAssignmentResponse> Items, int TotalCount)> GetPagedByClassIdAsync(Guid classId, int skip, int take, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<TeachingAssignmentResponse> Items, int TotalCount)> GetPagedBySubjectIdAsync(Guid subjectId, int skip, int take, CancellationToken cancellationToken = default);
}