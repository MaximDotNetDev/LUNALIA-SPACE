using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.QuizAssignments;

namespace SchoolJournal.Application.Features.Operations.QuizAssignments.GetQuizAssignmentsByQuizId;

public sealed record GetQuizAssignmentsByQuizIdQuery(Guid QuizId)
    : IRequest<ErrorOr<IReadOnlyCollection<QuizAssignmentResponse>>>;