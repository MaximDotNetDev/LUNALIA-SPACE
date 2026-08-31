using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.SchoolClasses.ActivateSchoolClass;

public sealed class ActivateSchoolClassCommandHandler(
    ISchoolClassRepository classRepository,
    IAuditContext auditContext)
    : IRequestHandler<ActivateSchoolClassCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(ActivateSchoolClassCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var rowVersionBytes = Convert.FromBase64String(request.RowVersionBase64);

        var updatedState = await classRepository.ChangeActiveStatusAsync(
            request.ClassId,
            true,
            rowVersionBytes,
            cancellationToken).ConfigureAwait(false);

        if (updatedState is null)
        {
            return Error.Conflict(
                code: "SchoolClass.ConcurrencyOrNotFound",
                description: "Клас не знайдено, або його дані вже були змінені іншим користувачем. Будь ласка, оновіть сторінку.");
        }

        auditContext.TrackNewState(updatedState);

        return Result.Success;
    }
}