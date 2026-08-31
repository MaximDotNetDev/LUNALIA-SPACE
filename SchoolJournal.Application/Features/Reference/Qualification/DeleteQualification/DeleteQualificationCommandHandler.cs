using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.Qualification.DeleteQualification;

public sealed class DeleteQualificationCommandHandler(
    IQualificationRepository qualificationRepository,
    IAuditContext auditContext)
    : IRequestHandler<DeleteQualificationCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeleteQualificationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var rowVersion = Convert.FromBase64String(request.RowVersionBase64);

        var oldState = await qualificationRepository.DeleteAsync(request.QualificationId, rowVersion, cancellationToken).ConfigureAwait(false);

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