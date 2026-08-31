using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.Subgroup.DeleteSubgroup;

public sealed record DeleteSubgroupCommand(
    Guid SubgroupId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;