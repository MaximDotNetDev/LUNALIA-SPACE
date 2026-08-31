namespace SchoolJournal.Contracts.DTOs.Core.Students;

public sealed record TransferStudentRequest(
    Guid NewClassId,
    string RowVersionBase64);