using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.TeachingAssignments.CreateTeachingAssignment;

public sealed record CreateTeachingAssignmentCommand(
    Guid TeacherId,
    Guid SubjectId,
    Guid ClassId,
    Guid? SubgroupId
) : IRequest<ErrorOr<Guid>>;