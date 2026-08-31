using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.Qualification.UpdateQualification;

public sealed class UpdateQualificationCommandHandler(
    IQualificationRepository qualificationRepository,
    IAuditContext auditContext)
    : IRequestHandler<UpdateQualificationCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateQualificationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await qualificationRepository.ExistsByNameExcludingIdAsync(request.QualificationName, request.QualificationId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "Qualification.NameConflict",
                description: $"Інша кваліфікація з назвою '{request.QualificationName}' вже існує.");
        }

        var qualification = new Domain.Entities.Reference.Qualification
        {
            QualificationId = request.QualificationId,
            QualificationName = request.QualificationName,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = Convert.FromBase64String(request.RowVersionBase64)
        };

        var oldState = await qualificationRepository.UpdateAsync(qualification, cancellationToken).ConfigureAwait(false);

        if (oldState is null)
        {
            return Error.Conflict(
                code: "Qualification.ConcurrencyConflict",
                description: "Запис не знайдено, або його було змінено/видалено іншим користувачем. Будь ласка, оновіть сторінку.");
        }

        auditContext.TrackOldState(oldState);

        var newState = await qualificationRepository.GetByIdAsync(request.QualificationId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return Result.Success;
    }
}