using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.Teachers.DeleteTeacher;

public sealed record DeleteTeacherCommand(
    Guid TeacherId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;