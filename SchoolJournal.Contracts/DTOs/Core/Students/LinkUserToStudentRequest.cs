namespace SchoolJournal.Contracts.DTOs.Core.Students;

public sealed record LinkUserToStudentRequest(
    Guid UserId,
    string RowVersionBase64);