using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.StudentParents;

namespace SchoolJournal.Application.Features.Core.StudentParents.GetStudentParentById;

public sealed record GetStudentParentByIdQuery(
    Guid StudentParentId
) : IRequest<ErrorOr<StudentParentResponse>>;