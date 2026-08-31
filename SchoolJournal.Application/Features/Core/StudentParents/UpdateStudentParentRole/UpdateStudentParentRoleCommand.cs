using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.StudentParents.UpdateStudentParentRole;

public sealed record UpdateStudentParentRoleCommand(
    Guid StudentParentId,
    string? Role
) : IRequest<ErrorOr<Success>>;