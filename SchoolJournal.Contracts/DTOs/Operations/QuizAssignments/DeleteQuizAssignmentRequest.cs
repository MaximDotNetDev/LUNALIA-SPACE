namespace SchoolJournal.Contracts.DTOs.Operations.QuizAssignments;

public sealed record DeleteQuizAssignmentRequest(
    string RowVersionBase64
);