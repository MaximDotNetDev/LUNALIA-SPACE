using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Subject.UpdateSubject;

public sealed class UpdateSubjectCommandHandler(
    ISubjectRepository subjectRepository,
    IAuditContext auditContext)
    : IRequestHandler<UpdateSubjectCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateSubjectCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var oldState = await subjectRepository.GetByIdAsync(request.SubjectId, cancellationToken).ConfigureAwait(false);
        if (oldState is null)
        {
            return Error.NotFound(
                code: "Subject.NotFound",
                description: $"Предмет з ідентифікатором '{request.SubjectId}' не знайдено.");
        }

        if (await subjectRepository.ExistsByNameExcludingIdAsync(request.SubjectName, request.SubjectId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "Subject.NameConflict",
                description: $"Інший предмет з назвою '{request.SubjectName}' вже існує.");
        }

        auditContext.TrackOldState(oldState);

        var subjectToUpdate = oldState with
        {
            SubjectName = request.SubjectName,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var updatedSubject = await subjectRepository.UpdateAsync(subjectToUpdate, cancellationToken).ConfigureAwait(false);

        if (updatedSubject is not null)
        {
            auditContext.TrackNewState(updatedSubject);
        }

        return Result.Success;
    }
}