namespace SchoolJournal.Contracts.DTOs.Operations.TeacherSubstitutions;

public sealed record DeleteTeacherSubstitutionRequest(
    string RowVersionBase64
);