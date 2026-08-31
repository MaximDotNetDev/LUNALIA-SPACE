namespace SchoolJournal.Contracts.DTOs.Operations.Quizzes;

public sealed record SubjectQuizzesRequest(
    int PageNumber = 1,
    int PageSize = 10
);