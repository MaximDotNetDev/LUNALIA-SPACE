namespace SchoolJournal.Contracts.DTOs.Operations.TeachingAssignments;

public sealed record UpdateTeachingAssignmentRequest(
    Guid TeacherId,
    Guid SubjectId,
    Guid ClassId,
    Guid? SubgroupId,
    string RowVersionBase64
);