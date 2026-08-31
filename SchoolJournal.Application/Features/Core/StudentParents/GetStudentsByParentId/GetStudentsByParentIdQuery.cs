using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.StudentParents;

namespace SchoolJournal.Application.Features.Core.StudentParents.GetStudentsByParentId;

public sealed record GetStudentsByParentIdQuery(
    Guid ParentId
) : IRequest<ErrorOr<IEnumerable<ParentStudentDetailResponse>>>;