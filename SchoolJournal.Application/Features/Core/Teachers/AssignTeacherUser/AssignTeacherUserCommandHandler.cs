using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Teachers.AssignTeacherUser;

public sealed class AssignTeacherUserCommandHandler(
    ITeacherRepository teacherRepository,
    IAuditContext auditContext)
    : IRequestHandler<AssignTeacherUserCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(AssignTeacherUserCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. Перевірка існування вчителя
        var currentTeacher = await teacherRepository.GetByIdAsync(request.TeacherId, cancellationToken).ConfigureAwait(false);
        if (currentTeacher is null)
        {
            return Error.NotFound("Teacher.NotFound", "Профіль вчителя не знайдено.");
        }

        // 2. Якщо UserId надано, перевіряємо чи він не зайнятий іншим вчителем
        if (request.UserId.HasValue &&
            await teacherRepository.IsUserAssignedToAnotherTeacherAsync(request.UserId.Value, request.TeacherId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict("Teacher.UserAlreadyAssigned", "Цей користувач вже прив'язаний до іншого вчителя.");
        }

        auditContext.TrackOldState(currentTeacher);

        // 3. Оновлення стану
        var rowVersion = Convert.FromBase64String(request.RowVersionBase64);
        var updatedTeacher = currentTeacher with
        {
            UserId = request.UserId,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = rowVersion
        };

        var result = await teacherRepository.AssignUserAsync(updatedTeacher, cancellationToken).ConfigureAwait(false);

        if (result is null)
        {
            return Error.Conflict("Teacher.ConcurrencyConflict", "Конфлікт паралелізму. Дані застаріли.");
        }

        auditContext.TrackNewState(result);

        return Result.Success;
    }
}