namespace SchoolJournal.Contracts.DTOs.Core.SchoolClasses;

public sealed record SchoolClassItemResponse(
    Guid ClassId,
    string ClassName,
    int GradeLevel,
    string AcademicYear,
    string HomeroomTeacherFullName
);