using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.StudentSubgroups;

namespace SchoolJournal.Application.Features.Core.StudentSubgroups.GetStudentSubgroupById;

public sealed record GetStudentSubgroupByIdQuery(
    Guid Id
) : IRequest<ErrorOr<StudentSubgroupResponse>>;