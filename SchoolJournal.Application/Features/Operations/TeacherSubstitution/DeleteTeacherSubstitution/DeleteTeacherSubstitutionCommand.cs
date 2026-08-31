using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.TeacherSubstitution.DeleteTeacherSubstitution;

public sealed record DeleteTeacherSubstitutionCommand(
    Guid SubstitutionId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;