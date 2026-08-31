namespace SchoolJournal.Contracts.DTOs.Operations.Lessons;

public sealed record UpdateLessonTopicAndHomeworkRequest(
    string? LessonTopic,
    string? Homework,
    string RowVersionBase64
);