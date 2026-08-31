namespace SchoolJournal.Contracts.DTOs.Communications.Announcements;

public sealed record ToggleAnnouncementStatusRequest(
    string RowVersionBase64
);