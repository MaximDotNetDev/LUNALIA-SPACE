using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.StudentParents;

namespace SchoolJournal.Application.Features.Core.StudentParents.GetParentsByStudentId;

public sealed record GetParentsByStudentIdQuery(
    Guid StudentId
) : IRequest<ErrorOr<IEnumerable<StudentParentDetailResponse>>>;