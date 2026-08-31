using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;
using SchoolJournal.Domain.Entities.Operations.IRepositories;

namespace SchoolJournal.Application.Features.Operations.Quizzes.GetQuizzesBySubject;

public sealed class GetQuizzesBySubjectQueryHandler(
    IQuizRepository quizRepository)
    : IRequestHandler<GetQuizzesBySubjectQuery, ErrorOr<PagedResponse<QuizResponse>>>
{
    public async Task<ErrorOr<PagedResponse<QuizResponse>>> Handle(GetQuizzesBySubjectQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await quizRepository.SubjectExistsAsync(request.SubjectId, cancellationToken).ConfigureAwait(false))
        {
            return Error.NotFound(
                code: "Quiz.SubjectNotFound",
                description: $"Предмета з ID '{request.SubjectId}' не знайдено.");
        }

        var (items, totalCount) = await quizRepository.GetPagedBySubjectIdAsync(
            request.SubjectId,
            request.PageRequest.Skip,
            request.PageRequest.PageSize,
            cancellationToken).ConfigureAwait(false);

        var quizResponses = items.Select(quiz => new QuizResponse(
            quiz.QuizId,
            quiz.TeacherId,
            quiz.SubjectId,
            quiz.Title,
            Convert.ToBase64String(quiz.RowVersion.ToArray()),
            quiz.CreatedAt,
            quiz.UpdatedAt
        )).ToList();

        return new PagedResponse<QuizResponse>(
            quizResponses,
            request.PageRequest.PageNumber,
            request.PageRequest.PageSize,
            totalCount);
    }
}