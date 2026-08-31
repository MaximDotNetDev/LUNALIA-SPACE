using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;

namespace SchoolJournal.Application.Features.AiGeneration.GenerateMultipleChoice;

public sealed class GenerateMultipleChoiceCommandHandler(
    IAiQuizGenerator aiQuizGenerator)
    : IRequestHandler<GenerateMultipleChoiceCommand, ErrorOr<GeneratedQuizResponse>>
{
    public async Task<ErrorOr<GeneratedQuizResponse>> Handle(GenerateMultipleChoiceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await aiQuizGenerator.GenerateMultipleChoiceAsync(
            request.Text,
            request.TotalQuestions,
            request.MultiAnswerCount,
            request.JudgmentCount,
            request.PointsPerQuestion,
            cancellationToken).ConfigureAwait(false);
    }
}