using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.QuizQuestions;

namespace SchoolJournal.Application.Features.Operations.QuizQuestions.ReorderQuizQuestions;

public sealed record ReorderQuizQuestionsCommand(
    Guid QuizId,
    IReadOnlyCollection<ReorderItem> Items
) : IRequest<ErrorOr<Success>>;