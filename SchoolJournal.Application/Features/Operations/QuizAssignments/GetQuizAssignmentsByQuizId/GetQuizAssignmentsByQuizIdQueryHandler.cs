using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Contracts.DTOs.Operations.QuizAssignments;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.QuizAssignments.GetQuizAssignmentsByQuizId;

public sealed class GetQuizAssignmentsByQuizIdQueryHandler(
    IQuizAssignmentRepository quizAssignmentRepository,
    IQuizRepository quizRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetQuizAssignmentsByQuizIdQuery, ErrorOr<IReadOnlyCollection<QuizAssignmentResponse>>>
{
    public async Task<ErrorOr<IReadOnlyCollection<QuizAssignmentResponse>>> Handle(
        GetQuizAssignmentsByQuizIdQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var quiz = await quizRepository.GetByIdAsync(request.QuizId, cancellationToken).ConfigureAwait(false);
        if (quiz is null || quiz.IsDeleted)
        {
            return Error.NotFound(
                code: "QuizAssignment.QuizNotFound",
                description: "Тест не знайдено або його було видалено.");
        }

        var userRole = currentUserService.GetUserRole();
        if (userRole == RoleType.Teacher)
        {
            var userId = currentUserService.GetUserId();
            if (quiz.TeacherId != userId)
            {
                return Error.Forbidden(
                    code: "QuizAssignment.Forbidden",
                    description: "Ви не є автором цього тесту, тому не можете переглядати його призначення.");
            }
        }

        var assignments = await quizAssignmentRepository.GetActiveByQuizIdAsync(request.QuizId, cancellationToken).ConfigureAwait(false);

        var response = assignments.Select(a => new QuizAssignmentResponse(
                    a.AssignmentId,
                    a.QuizId,
                    a.ClassId,
                    a.AssignedDate,
                    a.DueDate,
                    Convert.ToBase64String(a.RowVersion.ToArray()),
                    a.QuizTitle, 
                    a.ClassName  
                )).ToList().AsReadOnly();

        return response;
    }
}