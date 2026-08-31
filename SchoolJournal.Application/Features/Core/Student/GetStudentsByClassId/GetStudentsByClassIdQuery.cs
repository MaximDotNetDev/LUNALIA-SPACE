using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Students;

namespace SchoolJournal.Application.Features.Core.Student.GetStudentsByClassId;

public sealed record GetStudentsByClassIdQuery(Guid ClassId) : IRequest<ErrorOr<IEnumerable<StudentLookupResponse>>>;