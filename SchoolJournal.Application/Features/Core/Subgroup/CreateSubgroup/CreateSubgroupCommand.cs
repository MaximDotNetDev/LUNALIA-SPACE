using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.Subgroup.CreateSubgroup;

public sealed record CreateSubgroupCommand(
    Guid ClassId,
    Guid SubjectId,
    string SubgroupName
) : IRequest<ErrorOr<Guid>>;