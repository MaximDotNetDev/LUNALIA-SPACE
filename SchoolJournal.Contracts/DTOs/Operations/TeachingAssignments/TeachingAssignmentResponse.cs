namespace SchoolJournal.Contracts.DTOs.Operations.TeachingAssignments;

public sealed record TeachingAssignmentResponse(
    Guid AssignmentId,
    Guid TeacherId,
    string TeacherFullName,
    Guid SubjectId,
    string SubjectName,
    Guid ClassId,
    string ClassName,
    Guid? SubgroupId,
    string? SubgroupName,
    bool IsActive,
    string RowVersionBase64
);