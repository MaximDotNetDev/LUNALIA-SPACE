using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.Teachers.ToggleTeacherStatus;

public sealed record ToggleTeacherStatusCommand(
    Guid TeacherId,
    bool IsActive,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;