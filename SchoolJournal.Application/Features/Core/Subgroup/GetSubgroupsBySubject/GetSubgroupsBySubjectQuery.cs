namespace SchoolJournal.Application.Features.Core.Subgroup.GetSubgroupsBySubject;

using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Subgroups;

public sealed record GetSubgroupsBySubjectQuery(Guid ClassId, Guid SubjectId)
    : IRequest<ErrorOr<IReadOnlyCollection<SubgroupResponse>>>;