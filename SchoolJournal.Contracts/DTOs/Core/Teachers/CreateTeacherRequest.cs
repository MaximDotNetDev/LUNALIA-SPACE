namespace SchoolJournal.Contracts.DTOs.Core.Teachers;

public sealed record CreateTeacherRequest(
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
    Guid PositionId,
    Guid QualificationId,
    Guid? PedagogicalTitleId,
    Guid? UserId
);