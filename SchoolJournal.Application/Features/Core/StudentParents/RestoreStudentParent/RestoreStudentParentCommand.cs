using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.StudentParents.RestoreStudentParent;

public sealed record RestoreStudentParentCommand(
    Guid StudentParentId
) : IRequest<ErrorOr<Success>>;