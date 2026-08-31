using SchoolJournal.Contracts.DTOs.Core.Students;

namespace SchoolJournal.Contracts.DTOs.Core.StudentParents;

public sealed record ParentStudentDetailResponse(
    Guid StudentParentId,
    string? Role,
    StudentResponse Student
);