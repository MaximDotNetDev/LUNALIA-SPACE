using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.TeacherSubstitution.UpdateTeacherSubstitution;

public sealed record UpdateTeacherSubstitutionCommand(
    Guid SubstitutionId,
    Guid AssignmentId,
    Guid SubstituteTeacherId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;