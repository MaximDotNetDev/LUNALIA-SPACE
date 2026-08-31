using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.TeacherSubstitution.CreateTeacherSubstitution;

public sealed record CreateTeacherSubstitutionCommand(
    Guid AssignmentId,
    Guid SubstituteTeacherId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate
) : IRequest<ErrorOr<Guid>>;