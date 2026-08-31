namespace SchoolJournal.Contracts.DTOs.Operations.TeacherSubstitutions;

public sealed record UpdateTeacherSubstitutionRequest(
    Guid AssignmentId,
    Guid SubstituteTeacherId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string RowVersionBase64
);