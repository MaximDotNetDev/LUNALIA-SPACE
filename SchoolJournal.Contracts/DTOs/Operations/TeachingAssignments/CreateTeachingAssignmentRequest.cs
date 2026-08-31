namespace SchoolJournal.Contracts.DTOs.Operations.TeachingAssignments;

public sealed record CreateTeachingAssignmentRequest(
    Guid TeacherId,
    Guid SubjectId,
    Guid ClassId,
    Guid? SubgroupId
);