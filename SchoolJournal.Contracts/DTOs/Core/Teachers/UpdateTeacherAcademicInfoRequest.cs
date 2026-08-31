namespace SchoolJournal.Contracts.DTOs.Core.Teachers;

public sealed record UpdateTeacherAcademicInfoRequest(
    Guid PositionId,
    Guid QualificationId,
    Guid? PedagogicalTitleId,
    decimal? Workload,
    string RowVersionBase64
);