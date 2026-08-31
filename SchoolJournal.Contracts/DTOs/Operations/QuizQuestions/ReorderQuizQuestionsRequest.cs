namespace SchoolJournal.Contracts.DTOs.Operations.QuizQuestions;

public sealed record ReorderItem(
    Guid QuestionId,
    int OrderIndex
);

public sealed record ReorderQuizQuestionsRequest(
    IReadOnlyCollection<ReorderItem> Items
);