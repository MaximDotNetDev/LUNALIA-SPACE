using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;
using System.Text.Json;

namespace SchoolJournal.Application.Features.Operations.QuizQuestions.ReorderQuizQuestions;

public sealed class ReorderQuizQuestionsCommandHandler(
    IQuizRepository quizRepository,
    IQuizQuestionRepository quizQuestionRepository,
    ICurrentUserService currentUserService,
    IAuditContext auditContext)
    : IRequestHandler<ReorderQuizQuestionsCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(ReorderQuizQuestionsCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var quiz = await quizRepository.GetByIdAsync(request.QuizId, cancellationToken).ConfigureAwait(false);
        if (quiz is null || quiz.IsDeleted)
        {
            return Error.NotFound(
                code: "Quiz.NotFound",
                description: $"Тест з ідентифікатором '{request.QuizId}' не знайдено.");
        }

        var userRole = currentUserService.GetUserRole();
        if (userRole == RoleType.Teacher)
        {
            var currentUserId = currentUserService.GetUserId();
            if (quiz.TeacherId != currentUserId)
            {
                return Error.Forbidden(
                    code: "Quiz.OwnershipViolation",
                    description: "У вас немає прав на зміну порядку питань цього тесту, оскільки ви не є його автором.");
            }
        }

        var ordersJson = JsonSerializer.Serialize(request.Items);

        var isSuccess = await quizQuestionRepository.ReorderAsync(request.QuizId, ordersJson, cancellationToken).ConfigureAwait(false);
        if (!isSuccess)
        {
            return Error.Conflict(
                code: "QuizQuestions.ReorderFailed",
                description: "Не вдалося змінити порядок питань. Перевірте актуальність даних та приналежність питань до цього тесту.");
        }

        auditContext.TrackNewState(new { request.QuizId, ReorderedItems = request.Items });

        return Result.Success;
    }
}