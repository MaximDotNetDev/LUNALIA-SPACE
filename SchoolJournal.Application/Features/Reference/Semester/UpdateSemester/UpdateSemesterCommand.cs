using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.Semester.UpdateSemester;

public sealed record UpdateSemesterCommand(
    Guid SemesterId,
    string SemesterName,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;