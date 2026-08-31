namespace SchoolJournal.Contracts.DTOs.Operations.Lessons;

public sealed record DeleteLessonRequest(
    string RowVersionBase64
);