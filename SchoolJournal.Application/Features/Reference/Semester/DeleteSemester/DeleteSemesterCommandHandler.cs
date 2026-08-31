using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.Semester.DeleteSemester;

public sealed class DeleteSemesterCommandHandler(
    ISemesterRepository semesterRepository,
    IAuditContext auditContext)
    : IRequestHandler<DeleteSemesterCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeleteSemesterCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var rowVersion = Convert.FromBase64String(request.RowVersionBase64);

        var oldSemester = await semesterRepository.GetByIdAsync(request.SemesterId, cancellationToken).ConfigureAwait(false);
        if (oldSemester is not null)
        {
            auditContext.TrackOldState(oldSemester);
        }

        var oldSemesterFromDb = await semesterRepository.DeleteAsync(request.SemesterId, rowVersion, cancellationToken).ConfigureAwait(false);

        if (oldSemesterFromDb is null)
        {
            return Error.Conflict(
                code: "Semester.ConcurrencyConflict",
                description: "Неможливо видалити семестр. Дані були змінені або семестр вже видалено іншим користувачем.");
        }

        auditContext.TrackOldState(oldSemesterFromDb);

        var newState = await semesterRepository.GetByIdAsync(request.SemesterId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return Result.Success;
    }
}