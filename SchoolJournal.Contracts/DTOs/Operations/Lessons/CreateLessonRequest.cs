namespace SchoolJournal.Contracts.DTOs.Operations.Lessons;

public sealed record CreateLessonRequest(
    Guid AssignmentId,
    DateTimeOffset LessonDate,
    string? LessonTopic,
    string? Homework,
    Guid LessonTypeId,
    Guid PeriodId,
    Guid RoomId,
    Guid SemesterId
);