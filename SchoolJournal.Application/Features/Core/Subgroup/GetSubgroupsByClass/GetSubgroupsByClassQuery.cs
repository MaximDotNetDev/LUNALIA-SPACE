using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Subgroups;

namespace SchoolJournal.Application.Features.Core.Subgroup.GetSubgroupsByClass;

public sealed record GetSubgroupsByClassQuery(Guid ClassId)
    : IRequest<ErrorOr<IReadOnlyCollection<SubgroupResponse>>>;