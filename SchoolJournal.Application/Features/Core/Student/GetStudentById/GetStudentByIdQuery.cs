using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Students;

namespace SchoolJournal.Application.Features.Core.Student.GetStudentById;

public sealed record GetStudentByIdQuery(Guid Id) : IRequest<ErrorOr<StudentResponse>>;