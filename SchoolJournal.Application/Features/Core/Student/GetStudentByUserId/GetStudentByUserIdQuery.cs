using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Students;

namespace SchoolJournal.Application.Features.Core.Student.GetStudentByUserId;

public sealed record GetStudentByUserIdQuery(Guid UserId) : IRequest<ErrorOr<StudentResponse>>;