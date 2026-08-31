namespace SchoolJournal.Contracts.DTOs.Operations.QuizAssignments;

public sealed record UpdateQuizAssignmentDueDateRequest(
    DateTimeOffset? DueDate,
    string RowVersionBase64
);