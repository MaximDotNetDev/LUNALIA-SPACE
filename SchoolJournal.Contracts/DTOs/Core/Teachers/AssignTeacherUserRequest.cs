namespace SchoolJournal.Contracts.DTOs.Core.Teachers;

public sealed record AssignTeacherUserRequest(
    Guid? UserId,
    string RowVersionBase64
);