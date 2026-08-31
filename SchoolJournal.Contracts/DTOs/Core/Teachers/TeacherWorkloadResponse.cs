namespace SchoolJournal.Contracts.DTOs.Core.Teachers;

public sealed record TeacherWorkloadResponse(
    Guid TeacherId,
    string FullName,
    string PositionName,
    decimal Workload,
    bool IsActive
);