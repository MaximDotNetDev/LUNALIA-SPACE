namespace SchoolJournal.Contracts.DTOs.Communications.Announcements;

public sealed record UpdateAnnouncementRequest(
    string Title,
    string Content,
    DateTimeOffset? ExpirationDate,
    string RowVersionBase64
);