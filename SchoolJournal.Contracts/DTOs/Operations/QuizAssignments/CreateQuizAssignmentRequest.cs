namespace SchoolJournal.Contracts.DTOs.Operations.QuizAssignments;

public sealed record CreateQuizAssignmentRequest(
    Guid QuizId,
    Guid ClassId,
    DateTimeOffset? DueDate
);