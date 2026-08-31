using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;

namespace SchoolJournal.Application.Features.AiGeneration.GenerateMultipleChoice;

public sealed record GenerateMultipleChoiceCommand(
    string Text,
    int TotalQuestions,
    int MultiAnswerCount,
    int JudgmentCount,
    int PointsPerQuestion
) : IRequest<ErrorOr<GeneratedQuizResponse>>;