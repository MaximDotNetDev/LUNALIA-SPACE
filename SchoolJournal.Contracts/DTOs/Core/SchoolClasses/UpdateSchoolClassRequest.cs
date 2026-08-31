namespace SchoolJournal.Contracts.DTOs.Core.SchoolClasses;

public sealed record UpdateSchoolClassRequest(
    string ClassName,
    int GradeLevel,
    string AcademicYear,
    Guid HomeroomTeacherId,
    string RowVersionBase64
);