using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.Teachers.AssignTeacherUser;

public sealed record AssignTeacherUserCommand(
    Guid TeacherId,
    Guid? UserId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;