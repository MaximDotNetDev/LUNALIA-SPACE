namespace SchoolJournal.Contracts.DTOs.Operations.Lessons;

public sealed record LessonResponse(
    Guid LessonId,
    Guid AssignmentId,
    DateTimeOffset LessonDate,
    string? LessonTopic,
    string? Homework,
    Guid LessonTypeId,
    string LessonTypeName, 
    Guid PeriodId,
    string PeriodName,     
    Guid RoomId,
    string RoomName,      
    Guid SemesterId,
    string RowVersionBase64
);