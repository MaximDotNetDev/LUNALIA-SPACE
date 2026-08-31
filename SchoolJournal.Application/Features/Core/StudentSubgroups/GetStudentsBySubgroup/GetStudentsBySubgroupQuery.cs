using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.StudentSubgroups;

namespace SchoolJournal.Application.Features.Core.StudentSubgroups.GetStudentsBySubgroup;

public sealed record GetStudentsBySubgroupQuery(
    Guid SubgroupId
) : IRequest<ErrorOr<SubgroupStudentsDetail>>;