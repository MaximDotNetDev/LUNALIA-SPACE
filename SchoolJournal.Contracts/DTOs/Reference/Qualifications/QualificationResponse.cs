namespace SchoolJournal.Contracts.DTOs.Reference.Qualifications;

public sealed record QualificationResponse(
    Guid QualificationId,
    string QualificationName,
    string RowVersionBase64
);