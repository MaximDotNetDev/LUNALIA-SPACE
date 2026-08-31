using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.StudentSubgroups;

namespace SchoolJournal.Application.Features.Core.StudentSubgroups.GetSubgroupsByStudent;

public sealed record GetSubgroupsByStudentQuery(
    Guid StudentId
) : IRequest<ErrorOr<StudentSubgroupsDetail>>;