using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.StudentSubgroups;

namespace SchoolJournal.Application.Features.Core.StudentSubgroups.GetAvailableStudents;

public sealed record GetAvailableStudentsQuery(
    Guid SubgroupId
) : IRequest<ErrorOr<IEnumerable<AvailableStudentModel>>>;