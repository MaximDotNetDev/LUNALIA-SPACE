using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Reference.Semesters;

namespace SchoolJournal.Application.Features.Reference.Semester.GetSemesterById;

public sealed record GetSemesterByIdQuery(Guid SemesterId) : IRequest<ErrorOr<SemesterResponse>>;