using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.Semester.RestoreSemester;

public sealed record RestoreSemesterCommand(
    Guid SemesterId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;