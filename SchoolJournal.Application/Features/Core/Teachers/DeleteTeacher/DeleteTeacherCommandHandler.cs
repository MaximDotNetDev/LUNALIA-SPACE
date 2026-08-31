using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Teachers.DeleteTeacher;

public sealed class DeleteTeacherCommandHandler(
    ITeacherRepository teacherRepository,
    IAuditContext auditContext)
    : IRequestHandler<DeleteTeacherCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeleteTeacherCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var currentTeacher = await teacherRepository.GetByIdAsync(request.TeacherId, cancellationToken).ConfigureAwait(false);
        if (currentTeacher is null)
        {
            return Error.NotFound("Teacher.NotFound", "Профіль вчителя не знайдено.");
        }

        auditContext.TrackOldState(currentTeacher);

        var rowVersionBytes = Convert.FromBase64String(request.RowVersionBase64);

        var result = await teacherRepository.DeleteAsync(request.TeacherId, rowVersionBytes, cancellationToken).ConfigureAwait(false);

        if (result is null)
        {
            return Error.Conflict("Teacher.ConcurrencyConflict", "Не вдалося видалити профіль через конфлікт версій (дані були змінені іншим користувачем).");
        }

        auditContext.TrackNewState(result);

        return Result.Success;
    }
}