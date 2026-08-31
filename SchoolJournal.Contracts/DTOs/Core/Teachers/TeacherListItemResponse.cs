namespace SchoolJournal.Contracts.DTOs.Core.Teachers;

public sealed record TeacherListItemResponse(
    Guid TeacherId,
    string LastName,
    string FirstName,
    string? MiddleName,
    string? Phone,
    Guid PositionId,
    string PositionName,
    Guid QualificationId,
    string QualificationName,
    bool IsActive,
    Guid? UserId,
    string? Login
);