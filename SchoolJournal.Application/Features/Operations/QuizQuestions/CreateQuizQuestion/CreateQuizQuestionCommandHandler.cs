using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.QuizQuestions.CreateQuizQuestion;

public sealed class CreateQuizQuestionCommandHandler(
    IQuizRepository quizRepository,
    IQuizQuestionRepository quizQuestionRepository,
    ICurrentUserService currentUserService,
    IAuditContext auditContext)
    : IRequestHandler<CreateQuizQuestionCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateQuizQuestionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var quiz = await quizRepository.GetByIdAsync(request.QuizId, cancellationToken).ConfigureAwait(false);
        if (quiz is null || quiz.IsDeleted)
        {
            return Error.NotFound(
                code: "Quiz.NotFound",
                description: $"Тест із заданим ідентифікатором '{request.QuizId}' не знайдено.");
        }

        var userRole = currentUserService.GetUserRole();
        if (userRole == RoleType.Teacher)
        {
            var currentUserId = currentUserService.GetUserId();
            if (quiz.TeacherId != currentUserId)
            {
                return Error.Forbidden(
                    code: "Quiz.OwnershipViolation",
                    description: "У вас немає прав на додавання питань до цього тесту, оскільки ви не є його автором.");
            }
        }

        var nextOrderIndex = await quizQuestionRepository.GetNextOrderIndexAsync(request.QuizId, cancellationToken).ConfigureAwait(false);

        var question = new QuizQuestion
        {
            QuestionId = Guid.NewGuid(),
            QuizId = request.QuizId,
            OrderIndex = nextOrderIndex,
            QuestionText = request.QuestionText,
            QuestionType = request.QuestionType,
            ContentJson = request.ContentJson,
            Points = request.Points,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = null
        };

        var questionId = await quizQuestionRepository.AddAsync(question, cancellationToken).ConfigureAwait(false);

        var newState = await quizQuestionRepository.GetByIdAsync(questionId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return questionId;
    }
}