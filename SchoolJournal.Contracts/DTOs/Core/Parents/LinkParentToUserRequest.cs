namespace SchoolJournal.Contracts.DTOs.Core.Parents;

public sealed record LinkParentToUserRequest(
    Guid UserId,
    string RowVersionBase64
);