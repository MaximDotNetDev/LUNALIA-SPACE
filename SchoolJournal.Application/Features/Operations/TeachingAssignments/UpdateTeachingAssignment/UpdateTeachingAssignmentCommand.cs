using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.TeachingAssignments.UpdateTeachingAssignment;

public sealed record UpdateTeachingAssignmentCommand(
    Guid AssignmentId,
    Guid TeacherId,
    Guid SubjectId,
    Guid ClassId,
    Guid? SubgroupId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;