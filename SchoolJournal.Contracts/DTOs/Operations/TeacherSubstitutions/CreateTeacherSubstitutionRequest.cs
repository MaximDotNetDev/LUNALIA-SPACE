namespace SchoolJournal.Contracts.DTOs.Operations.TeacherSubstitutions;

public sealed record CreateTeacherSubstitutionRequest(
    Guid AssignmentId,
    Guid SubstituteTeacherId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate
);