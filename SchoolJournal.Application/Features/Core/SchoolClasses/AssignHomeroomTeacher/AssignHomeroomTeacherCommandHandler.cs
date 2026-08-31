using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.SchoolClasses.AssignHomeroomTeacher;

public sealed class AssignHomeroomTeacherCommandHandler(
    ISchoolClassRepository classRepository,
    IAuditContext auditContext)
    : IRequestHandler<AssignHomeroomTeacherCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(AssignHomeroomTeacherCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await classRepository.TeacherExistsAsync(request.NewHomeroomTeacherId, cancellationToken).ConfigureAwait(false))
        {
            return Error.NotFound(
                code: "SchoolClass.TeacherNotFound",
                description: "Вказаного вчителя не знайдено.");
        }

        var rowVersionBytes = Convert.FromBase64String(request.RowVersionBase64);

        var updatedState = await classRepository.UpdateHomeroomTeacherAsync(
            request.ClassId,
            request.NewHomeroomTeacherId,
            rowVersionBytes,
            cancellationToken).ConfigureAwait(false);

        if (updatedState is null)
        {
            return Error.Conflict(
                code: "SchoolClass.ConcurrencyOrNotFound",
                description: "Клас не знайдено, або його дані вже були змінені іншим користувачем. Будь ласка, оновіть сторінку.");
        }

        auditContext.TrackNewState(updatedState);

        return Result.Success;
    }
}