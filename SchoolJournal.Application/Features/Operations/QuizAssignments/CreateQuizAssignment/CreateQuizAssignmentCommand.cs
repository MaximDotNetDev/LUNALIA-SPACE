using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.QuizAssignments.CreateQuizAssignment;

public sealed record CreateQuizAssignmentCommand(
    Guid QuizId,
    Guid ClassId,
    DateTimeOffset? DueDate
) : IRequest<ErrorOr<Guid>>;