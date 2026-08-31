using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.SchoolClasses.AssignHomeroomTeacher;

public sealed record AssignHomeroomTeacherCommand(
    Guid ClassId,
    Guid NewHomeroomTeacherId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;