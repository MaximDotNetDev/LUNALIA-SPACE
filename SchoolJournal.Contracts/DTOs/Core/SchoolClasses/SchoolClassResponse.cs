namespace SchoolJournal.Contracts.DTOs.Core.SchoolClasses;

public sealed record SchoolClassResponse(
    Guid ClassId,
    string ClassName,
    int GradeLevel,
    string AcademicYear,
    Guid HomeroomTeacherId,
    string HomeroomTeacherFullName,
    bool IsActive,
    string RowVersionBase64
);