using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Subject.CreateSubject;

public sealed class CreateSubjectCommandHandler(
    ISubjectRepository subjectRepository,
    IAuditContext auditContext)
    : IRequestHandler<CreateSubjectCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateSubjectCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await subjectRepository.ExistsByNameAsync(request.SubjectName, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "Subject.NameConflict",
                description: $"Предмет з назвою '{request.SubjectName}' вже існує.");
        }

        var subject = new Domain.Entities.Core.Subject
        {
            SubjectName = request.SubjectName
        };

        var subjectId = await subjectRepository.AddAsync(subject, cancellationToken).ConfigureAwait(false);

        var newState = await subjectRepository.GetByIdAsync(subjectId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return subjectId;
    }
}