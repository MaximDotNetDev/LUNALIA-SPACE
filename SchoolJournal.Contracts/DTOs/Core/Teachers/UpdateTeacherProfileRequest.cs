namespace SchoolJournal.Contracts.DTOs.Core.Teachers;

public sealed record UpdateTeacherProfileRequest(
    string LastName,
    string FirstName,
    string? MiddleName,
    string? Phone,
    string? Specialization,
    DateTimeOffset? DateOfBirth,
    string Gender,
    string? EducationInfo,
    string? MeetLink,
    string RowVersionBase64
);