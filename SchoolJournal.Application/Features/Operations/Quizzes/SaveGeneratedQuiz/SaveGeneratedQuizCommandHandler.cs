using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.Quizzes.SaveGeneratedQuiz;

public sealed class SaveGeneratedQuizCommandHandler(
    IQuizRepository quizRepository,
    IQuizAssignmentRepository assignmentRepository,
    ICurrentUserService currentUserService,
    IAuditContext auditContext)
    : IRequestHandler<SaveGeneratedQuizCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(SaveGeneratedQuizCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userRole = currentUserService.GetUserRole();
        var currentUserId = currentUserService.GetUserId();

        if (userRole == RoleType.Teacher && currentUserId != request.TeacherId)
        {
            return Error.Forbidden(
                code: "Quiz.OwnershipViolation",
                description: "Вчитель може зберігати тести лише для себе.");
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

        if (!await assignmentRepository.TeacherTeachesClassAsync(request.TeacherId, request.ClassId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Validation(
                code: "Quiz.InvalidClassAssignment",
                description: "Ви не можете призначити тест класу, в якому не викладаєте.");
        }

        var quiz = new Quiz
        {
            TeacherId = request.TeacherId,
            SubjectId = request.SubjectId,
            Title = request.Title
        };

        var questions = request.Questions.Select(q => new QuizQuestion
        {
            QuizId = Guid.Empty,
            OrderIndex = q.OrderIndex,
            QuestionText = q.QuestionText,
            QuestionType = q.QuestionType,
            ContentJson = q.ContentJson,
            Points = q.Points
        }).ToList();

        var quizId = await quizRepository.AddGeneratedQuizAsync(quiz, questions, cancellationToken).ConfigureAwait(false);

        var assignment = new QuizAssignment
        {
            QuizId = quizId,
            ClassId = request.ClassId,
            AssignedDate = DateTimeOffset.UtcNow,
            DueDate = null
        };

        await assignmentRepository.AddAsync(assignment, cancellationToken).ConfigureAwait(false);

        var newState = await quizRepository.GetByIdAsync(quizId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return quizId;
    }
}