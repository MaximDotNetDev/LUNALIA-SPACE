using ErrorOr;
using MediatR;
using SchoolJournal.Domain.Entities.Reference.IRepositories;
using SchoolJournal.Application.Common.Interfaces;

namespace SchoolJournal.Application.Features.Reference.Semester.UpdateSemester;

public sealed class UpdateSemesterCommandHandler(
    ISemesterRepository semesterRepository,
    IAuditContext auditContext)
    : IRequestHandler<UpdateSemesterCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateSemesterCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await semesterRepository.ExistsByNameExcludingIdAsync(request.SemesterName, request.SemesterId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "Semester.NameConflict",
                description: $"Інший семестр з назвою '{request.SemesterName}' вже існує.");
        }

        if (await semesterRepository.HasOverlappingDatesExcludingIdAsync(request.StartDate, request.EndDate, request.SemesterId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "Semester.DatesOverlap",
                description: "Вказані дати перетинаються з іншим існуючим активним семестром.");
        }

        var oldSemester = await semesterRepository.GetByIdAsync(request.SemesterId, cancellationToken).ConfigureAwait(false);
        if (oldSemester is not null)
        {
            auditContext.TrackOldState(oldSemester);
        }

        var semester = new SchoolJournal.Domain.Entities.Reference.Semester
        {
            SemesterId = request.SemesterId,
            SemesterName = request.SemesterName,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = Convert.FromBase64String(request.RowVersionBase64)
        };

        var oldStateFromDb = await semesterRepository.UpdateAsync(semester, cancellationToken).ConfigureAwait(false);

        if (oldStateFromDb is null)
        {
            return Error.Conflict(
                code: "Semester.ConcurrencyConflict",
                description: "Дані семестру були змінені іншим користувачем або семестр не існує.");
        }

        auditContext.TrackOldState(oldStateFromDb);

        var newStateFromDb = await semesterRepository.GetByIdAsync(request.SemesterId, cancellationToken).ConfigureAwait(false);
        if (newStateFromDb is not null)
        {
            auditContext.TrackNewState(newStateFromDb);
        }

        return Result.Success;
    }
}