using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.Semester.CreateSemester;

public sealed record CreateSemesterCommand(
    string SemesterName,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate
) : IRequest<ErrorOr<Guid>>;