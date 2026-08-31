using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.QuizSubmissions;
using SchoolJournal.Domain.Entities.Operations.IRepositories;

namespace SchoolJournal.Application.Features.Operations.QuizSubmissions.GetAssignmentSubmissions;

public sealed class GetAssignmentSubmissionsQueryHandler(IQuizSubmissionRepository quizSubmissionRepository)
    : IRequestHandler<GetAssignmentSubmissionsQuery, ErrorOr<List<QuizSubmissionResultDto>>>
{
    public async Task<ErrorOr<List<QuizSubmissionResultDto>>> Handle(GetAssignmentSubmissionsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var submissions = await quizSubmissionRepository.GetAssignmentSubmissionsAsync(request.AssignmentId, cancellationToken).ConfigureAwait(false);

        // Мапимо доменну модель у DTO
        var dtos = submissions.Select(s => new QuizSubmissionResultDto(
            s.SubmissionId,
            s.StudentFullName,
            s.Score,
            s.MaxScore,
            s.SubmittedAt.LocalDateTime
        )).ToList();

        return dtos;
    }
}