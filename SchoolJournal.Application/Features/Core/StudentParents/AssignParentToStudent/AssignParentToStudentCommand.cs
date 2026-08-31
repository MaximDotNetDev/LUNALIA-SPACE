using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.StudentParents.AssignParentToStudent;

public sealed record AssignParentToStudentCommand(
    Guid StudentId,
    Guid ParentId,
    string? Role
) : IRequest<ErrorOr<Guid>>;