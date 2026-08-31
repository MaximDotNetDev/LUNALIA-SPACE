using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.Qualification.CreateQualification;

public sealed class CreateQualificationCommandHandler(
    IQualificationRepository qualificationRepository,
    IAuditContext auditContext)
    : IRequestHandler<CreateQualificationCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateQualificationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await qualificationRepository.ExistsByNameAsync(request.QualificationName, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "Qualification.NameConflict",
                description: $"Кваліфікація з назвою '{request.QualificationName}' вже існує.");
        }

        var qualification = new Domain.Entities.Reference.Qualification
        {
            QualificationName = request.QualificationName
        };

        var qualificationId = await qualificationRepository.AddAsync(qualification, cancellationToken).ConfigureAwait(false);

        var newState = await qualificationRepository.GetByIdAsync(qualificationId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return qualificationId;
    }
}