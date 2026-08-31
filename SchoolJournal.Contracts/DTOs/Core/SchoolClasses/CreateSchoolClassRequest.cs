namespace SchoolJournal.Contracts.DTOs.Core.SchoolClasses;

public sealed record CreateSchoolClassRequest(
    string ClassName,
    int GradeLevel,
    string AcademicYear,
    Guid HomeroomTeacherId
);