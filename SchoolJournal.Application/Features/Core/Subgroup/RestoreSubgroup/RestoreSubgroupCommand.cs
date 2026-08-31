using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.Subgroup.RestoreSubgroup;

public sealed record RestoreSubgroupCommand(
    Guid SubgroupId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;