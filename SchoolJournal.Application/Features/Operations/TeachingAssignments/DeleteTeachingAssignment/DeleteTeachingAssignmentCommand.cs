using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.TeachingAssignments.DeleteTeachingAssignment;

public sealed record DeleteTeachingAssignmentCommand(
    Guid AssignmentId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;