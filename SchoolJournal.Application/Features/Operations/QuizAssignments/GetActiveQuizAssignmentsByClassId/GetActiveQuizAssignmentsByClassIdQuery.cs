using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.QuizAssignments;

namespace SchoolJournal.Application.Features.Operations.QuizAssignments.GetActiveQuizAssignmentsByClassId;

public sealed record GetActiveQuizAssignmentsByClassIdQuery(Guid ClassId)
    : IRequest<ErrorOr<IReadOnlyCollection<QuizAssignmentResponse>>>;