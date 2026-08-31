namespace SchoolJournal.Application.Features.Core.Subgroup.GetSubgroupsList;

using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Core.Subgroups;

public sealed record GetSubgroupsListQuery(PageRequest PageRequest)
    : IRequest<ErrorOr<PagedResponse<SubgroupResponse>>>;