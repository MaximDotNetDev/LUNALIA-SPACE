using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Students;

namespace SchoolJournal.Application.Features.Core.Student.GetStudentHistory;

public sealed record GetStudentHistoryQuery(Guid StudentId) : IRequest<ErrorOr<IEnumerable<StudentHistoryResponse>>>;