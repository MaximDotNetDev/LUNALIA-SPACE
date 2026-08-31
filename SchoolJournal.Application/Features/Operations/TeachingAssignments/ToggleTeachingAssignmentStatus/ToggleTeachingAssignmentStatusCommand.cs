using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.TeachingAssignments.ToggleTeachingAssignmentStatus;

public sealed record ToggleTeachingAssignmentStatusCommand(
    Guid AssignmentId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;