namespace SchoolJournal.Contracts.DTOs.Core.Students;

public sealed record CreateStudentRequest(
    string LastName,
    string FirstName,
    string? MiddleName,
    DateTimeOffset? DateOfBirth,
    Guid ClassId,
    string? Gender,
    string? DocumentType,
    string? DocumentSeries,
    string? DocumentNumber,
    DateTimeOffset? EnrollmentDate,
    string? EnrollmentReason,
    string? Address,
    string? MedicalNotes,
    Guid? UserId);