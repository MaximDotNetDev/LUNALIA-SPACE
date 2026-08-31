namespace SchoolJournal.Contracts.DTOs.Communications.Announcements;

public sealed record CreateAnnouncementRequest(
    string Title,
    string Content,
    DateTimeOffset? ExpirationDate
);