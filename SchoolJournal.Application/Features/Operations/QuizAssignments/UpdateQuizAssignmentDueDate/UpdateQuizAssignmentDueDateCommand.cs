using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.QuizAssignments.UpdateQuizAssignmentDueDate;

public sealed record UpdateQuizAssignmentDueDateCommand(
    Guid AssignmentId,
    DateTimeOffset? DueDate,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;