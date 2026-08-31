using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.QuizAssignments.DeleteQuizAssignment;

public sealed record DeleteQuizAssignmentCommand(
    Guid AssignmentId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;