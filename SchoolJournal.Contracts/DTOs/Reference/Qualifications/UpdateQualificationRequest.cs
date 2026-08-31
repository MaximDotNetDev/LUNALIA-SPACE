namespace SchoolJournal.Contracts.DTOs.Reference.Qualifications;

public sealed record UpdateQualificationRequest(
    string QualificationName,
    string RowVersionBase64
);