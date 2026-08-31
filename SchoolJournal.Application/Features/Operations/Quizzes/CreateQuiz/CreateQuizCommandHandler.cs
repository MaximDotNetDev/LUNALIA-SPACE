using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.Quizzes.CreateQuiz;

public sealed class CreateQuizCommandHandler(
    IQuizRepository quizRepository,
    ICurrentUserService currentUserService,
    IAuditContext auditContext)
    : IRequestHandler<CreateQuizCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateQuizCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userRole = currentUserService.GetUserRole();
        var currentUserId = currentUserService.GetUserId();

        if (userRole == RoleType.Teacher && currentUserId != request.TeacherId)
        {
            return Error.Forbidden(
                code: "Quiz.OwnershipViolation",
                description: "Вчитель може створювати тести лише для себе.");
        }

        if (!await quizRepository.TeacherExistsAsync(request.TeacherId, cancellationToken).ConfigureAwait(false))
        {
            return Error.NotFound(
                code: "Quiz.TeacherNotFound",
                description: $"Вчителя з ID '{request.TeacherId}' не знайдено.");
        }

        if (!await quizRepository.SubjectExistsAsync(request.SubjectId, cancellationToken).ConfigureAwait(false))
        {
            return Error.NotFound(
                code: "Quiz.SubjectNotFound",
                description: $"Предмета з ID '{request.SubjectId}' не знайдено.");
        }

        var quiz = new Quiz
        {
            TeacherId = request.TeacherId,
            SubjectId = request.SubjectId,
            Title = request.Title
        };

        var quizId = await quizRepository.AddAsync(quiz, cancellationToken).ConfigureAwait(false);

        var newState = await quizRepository.GetByIdAsync(quizId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return quizId;
    }
}