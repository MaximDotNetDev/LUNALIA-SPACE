using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.Attendance.RecordAttendance;

public sealed record RecordAttendanceCommand(
    Guid LessonId,
    Guid StudentId,
    string Status,
    string? Comment
) : IRequest<ErrorOr<Guid>>;