namespace SchoolJournal.Contracts.DTOs.Operations.Quizzes;

public sealed record TeacherQuizzesRequest(
    int PageNumber = 1,
    int PageSize = 10
);