namespace SchoolJournal.Contracts.DTOs.Communications.Announcements;

public sealed record AnnouncementResponse(
    Guid AnnouncementId,
    string Title,
    string Content,
    Guid AuthorId,
    DateTimeOffset DateCreated,
    DateTimeOffset? ExpirationDate,
    bool IsActive,
    string RowVersionBase64
);