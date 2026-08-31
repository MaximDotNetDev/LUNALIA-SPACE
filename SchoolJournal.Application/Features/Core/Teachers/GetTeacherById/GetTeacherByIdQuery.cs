using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Teachers;

namespace SchoolJournal.Application.Features.Core.Teachers.GetTeacherById;

public sealed record GetTeacherByIdQuery(Guid TeacherId) : IRequest<ErrorOr<TeacherResponse>>;