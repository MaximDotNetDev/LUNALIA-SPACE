using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Teachers.ToggleTeacherStatus;

public sealed class ToggleTeacherStatusCommandHandler(
    ITeacherRepository teacherRepository,
    IAuditContext auditContext)
    : IRequestHandler<ToggleTeacherStatusCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(ToggleTeacherStatusCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var currentTeacher = await teacherRepository.GetByIdAsync(request.TeacherId, cancellationToken).ConfigureAwait(false);
        if (currentTeacher is null)
        {
            return Error.NotFound("Teacher.NotFound", "Профіль вчителя не знайдено.");
        }

        if (currentTeacher.IsActive == request.IsActive)
        {
            return Result.Success; // Статус вже відповідає бажаному
        }

        auditContext.TrackOldState(currentTeacher);

        var rowVersion = Convert.FromBase64String(request.RowVersionBase64);
        var updatedTeacher = currentTeacher with
        {
            IsActive = request.IsActive,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = rowVersion
        };

        var result = await teacherRepository.ToggleStatusAsync(updatedTeacher, cancellationToken).ConfigureAwait(false);

        if (result is null)
        {
            return Error.Conflict("Teacher.ConcurrencyConflict", "Не вдалося змінити статус через конфлікт версій.");
        }

        auditContext.TrackNewState(result);

        return Result.Success;
    }
}