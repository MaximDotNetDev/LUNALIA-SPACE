using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.Semester.DeleteSemester;

public sealed record DeleteSemesterCommand(
    Guid SemesterId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;