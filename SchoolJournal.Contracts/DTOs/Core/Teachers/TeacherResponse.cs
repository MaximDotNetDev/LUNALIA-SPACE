namespace SchoolJournal.Contracts.DTOs.Core.Teachers;

public sealed record TeacherResponse(
    Guid TeacherId,
    string LastName,
    string FirstName,
    string? MiddleName,
    string? Phone,
    string? Specialization,
    DateTimeOffset? DateOfBirth,
    string Gender,
    decimal? Workload,
    string? EducationInfo,
    string? MeetLink,
    Guid? UserId,
    Guid PositionId,
    string PositionName,
    Guid QualificationId,
    string QualificationName,
    Guid? PedagogicalTitleId,
    string? PedagogicalTitleName,
    bool IsActive,
    bool IsDeleted,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string RowVersionBase64
);