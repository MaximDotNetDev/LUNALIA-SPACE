using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Contracts.DTOs.Operations.QuizAssignments;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.QuizAssignments.GetQuizAssignmentById;

public sealed class GetQuizAssignmentByIdQueryHandler(
    IQuizAssignmentRepository quizAssignmentRepository,
    IQuizRepository quizRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetQuizAssignmentByIdQuery, ErrorOr<QuizAssignmentResponse>>
{
    public async Task<ErrorOr<QuizAssignmentResponse>> Handle(GetQuizAssignmentByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var assignment = await quizAssignmentRepository.GetByIdAsync(request.AssignmentId, cancellationToken).ConfigureAwait(false);
        if (assignment is null || assignment.IsDeleted)
        {
            return Error.NotFound(
                code: "QuizAssignment.NotFound",
                description: "Призначення тесту не знайдено або його було видалено.");
        }

        var userRole = currentUserService.GetUserRole();
        if (userRole == RoleType.Teacher)
        {
            var quiz = await quizRepository.GetByIdAsync(assignment.QuizId, cancellationToken).ConfigureAwait(false);
            var userId = currentUserService.GetUserId();

            if (quiz is null || quiz.TeacherId != userId)
            {
                return Error.Forbidden(
                    code: "QuizAssignment.Forbidden",
                    description: "Ви не маєте прав доступу до перегляду деталей цього призначення.");
            }
        }

        return new QuizAssignmentResponse(
                    assignment.AssignmentId,
                    assignment.QuizId,
                    assignment.ClassId,
                    assignment.AssignedDate,
                    assignment.DueDate,
                    Convert.ToBase64String(assignment.RowVersion.ToArray()),
                    string.Empty, 
                    string.Empty  
                );
    }
}