namespace SchoolJournal.Contracts.DTOs.Core.StudentParents;

public sealed record AssignParentToStudentRequest(
    Guid StudentId,
    Guid ParentId,
    string? Role
);