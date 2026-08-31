namespace SchoolJournal.Contracts.DTOs.Operations.QuizAssignments;

public sealed record QuizAssignmentResponse(
    Guid AssignmentId,
    Guid QuizId,
    Guid ClassId,
    DateTimeOffset AssignedDate,
    DateTimeOffset? DueDate,
    string RowVersionBase64,
    string QuizTitle,  
    string ClassName
);