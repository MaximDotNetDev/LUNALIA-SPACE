using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.Qualification.RestoreQualification;

public sealed class RestoreQualificationCommandHandler(
    IQualificationRepository qualificationRepository,
    IAuditContext auditContext)
    : IRequestHandler<RestoreQualificationCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(RestoreQualificationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var rowVersion = Convert.FromBase64String(request.RowVersionBase64);

        var oldState = await qualificationRepository.RestoreAsync(request.QualificationId, rowVersion, cancellationToken).ConfigureAwait(false);

        if (oldState is null)
        {
            return Error.Conflict(
                code: "Qualification.ConcurrencyConflict",
                description: "Запис не знайдено, або його було змінено/відновлено іншим користувачем. Будь ласка, оновіть сторінку.");
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