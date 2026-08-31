using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Teachers;

namespace SchoolJournal.Application.Features.Core.Teachers.GetTeacherByUserId;

public sealed record GetTeacherByUserIdQuery(Guid UserId) : IRequest<ErrorOr<TeacherResponse>>;