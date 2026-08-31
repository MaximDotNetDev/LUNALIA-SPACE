using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.Semester.RestoreSemester;

public sealed class RestoreSemesterCommandHandler(
    ISemesterRepository semesterRepository,
    IAuditContext auditContext)
    : IRequestHandler<RestoreSemesterCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(RestoreSemesterCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var semester = await semesterRepository.GetByIdAsync(request.SemesterId, cancellationToken).ConfigureAwait(false);

        if (semester is null)
        {
            return Error.NotFound(code: "Semester.NotFound", description: "Семестр не знайдено.");
        }

        if (!semester.IsDeleted)
        {
            return Error.Conflict(code: "Semester.NotDeleted", description: "Семестр вже є активним.");
        }

        if (await semesterRepository.ExistsByNameAsync(semester.SemesterName, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "Semester.NameConflict",
                description: $"Неможливо відновити. Активний семестр з назвою '{semester.SemesterName}' вже існує.");
        }

        if (await semesterRepository.HasOverlappingDatesAsync(semester.StartDate, semester.EndDate, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "Semester.DatesOverlap",
                description: "Неможливо відновити. Дати семестру перетинаються з іншим активним семестром.");
        }

        var rowVersion = Convert.FromBase64String(request.RowVersionBase64);

        var oldSemesterFromDb = await semesterRepository.RestoreAsync(request.SemesterId, rowVersion, cancellationToken).ConfigureAwait(false);

        if (oldSemesterFromDb is null)
        {
            return Error.Conflict(
                code: "Semester.ConcurrencyConflict",
                description: "Неможливо відновити семестр. Дані були змінені іншим користувачем.");
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