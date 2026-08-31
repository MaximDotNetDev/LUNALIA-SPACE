using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Subject.RestoreSubject;

public sealed class RestoreSubjectCommandHandler(
    ISubjectRepository subjectRepository,
    IAuditContext auditContext)
    : IRequestHandler<RestoreSubjectCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(RestoreSubjectCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var oldState = await subjectRepository.GetByIdAsync(request.SubjectId, cancellationToken).ConfigureAwait(false);
        if (oldState is null)
        {
            return Error.NotFound(
                code: "Subject.NotFound",
                description: $"Предмет з ідентифікатором '{request.SubjectId}' не знайдено.");
        }

        if (!oldState.IsDeleted)
        {
            return Error.Conflict(
                code: "Subject.AlreadyActive",
                description: $"Предмет '{oldState.SubjectName}' вже є активним.");
        }

        if (await subjectRepository.ExistsByNameExcludingIdAsync(oldState.SubjectName, request.SubjectId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "Subject.RestoreConflict",
                description: $"Неможливо відновити. Активний предмет з назвою '{oldState.SubjectName}' вже існує.");
        }

        auditContext.TrackOldState(oldState);

        var restoredSubject = await subjectRepository.RestoreAsync(request.SubjectId, cancellationToken).ConfigureAwait(false);

        if (restoredSubject is not null)
        {
            auditContext.TrackNewState(restoredSubject);
        }

        return Result.Success;
    }
}