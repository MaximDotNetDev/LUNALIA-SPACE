using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Subject.DeleteSubject;

public sealed class DeleteSubjectCommandHandler(
    ISubjectRepository subjectRepository,
    IAuditContext auditContext)
    : IRequestHandler<DeleteSubjectCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeleteSubjectCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var oldState = await subjectRepository.GetByIdAsync(request.SubjectId, cancellationToken).ConfigureAwait(false);
        if (oldState is null || oldState.IsDeleted)
        {
            return Error.NotFound(
                code: "Subject.NotFound",
                description: $"Активний предмет з ідентифікатором '{request.SubjectId}' не знайдено.");
        }

        auditContext.TrackOldState(oldState);

        var deletedSubject = await subjectRepository.DeleteAsync(request.SubjectId, cancellationToken).ConfigureAwait(false);

        if (deletedSubject is not null)
        {
            auditContext.TrackNewState(deletedSubject);
        }

        return Result.Success;
    }
}