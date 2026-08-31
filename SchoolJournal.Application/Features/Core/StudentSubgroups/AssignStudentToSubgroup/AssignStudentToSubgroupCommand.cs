using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.StudentSubgroups.AssignStudentToSubgroup;

public sealed record AssignStudentToSubgroupCommand(
    Guid StudentId,
    Guid SubgroupId
) : IRequest<ErrorOr<Guid>>;