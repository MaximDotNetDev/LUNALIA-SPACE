using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.Semester.CreateSemester;

public sealed class CreateSemesterCommandHandler(
    ISemesterRepository semesterRepository,
    IAuditContext auditContext)
    : IRequestHandler<CreateSemesterCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateSemesterCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await semesterRepository.ExistsByNameAsync(request.SemesterName, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "Semester.NameConflict",
                description: $"Семестр з назвою '{request.SemesterName}' вже існує.");
        }

        if (await semesterRepository.HasOverlappingDatesAsync(request.StartDate, request.EndDate, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "Semester.DatesOverlap",
                description: "Вказані дати перетинаються з існуючим активним семестром.");
        }

        var semester = new Domain.Entities.Reference.Semester
        {
            SemesterName = request.SemesterName,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        var semesterId = await semesterRepository.AddAsync(semester, cancellationToken).ConfigureAwait(false);

        var newState = await semesterRepository.GetByIdAsync(semesterId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return semesterId;
    }
}