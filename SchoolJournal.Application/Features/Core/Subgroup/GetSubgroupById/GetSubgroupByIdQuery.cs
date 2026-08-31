using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Subgroups;

namespace SchoolJournal.Application.Features.Core.Subgroup.GetSubgroupById;

public sealed record GetSubgroupByIdQuery(Guid SubgroupId) : IRequest<ErrorOr<SubgroupResponse>>;