namespace SchoolJournal.Contracts.DTOs.Operations.Quizzes;

public sealed record QuizzesSearchRequest(
    string? SearchTerm = null,
    int PageNumber = 1,
    int PageSize = 10
);