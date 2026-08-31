namespace SchoolJournal.Contracts.DTOs.Communications.Announcements;

public sealed record DeleteAnnouncementRequest(
    string RowVersionBase64
);