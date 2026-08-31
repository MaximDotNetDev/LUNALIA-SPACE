using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.Subgroup.UpdateSubgroup;

public sealed record UpdateSubgroupCommand(
    Guid SubgroupId,
    string SubgroupName,
    bool IsActive,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;