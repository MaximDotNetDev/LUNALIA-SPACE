namespace SchoolJournal.Contracts.DTOs.Operations.TeacherSubstitutions;

public sealed record TeacherSubstitutionResponse(
    Guid SubstitutionId,
    Guid AssignmentId,
    Guid SubstituteTeacherId,
    string SubstituteTeacherFullName,
    string MainTeacherFullName,
    string SubjectName,
    string ClassName,
    string? SubgroupName,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string RowVersionBase64
);