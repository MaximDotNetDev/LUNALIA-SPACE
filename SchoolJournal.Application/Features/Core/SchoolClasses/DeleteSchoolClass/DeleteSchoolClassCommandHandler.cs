using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.SchoolClasses.DeleteSchoolClass;

public sealed class DeleteSchoolClassCommandHandler(
    ISchoolClassRepository classRepository,
    IAuditContext auditContext)
    : IRequestHandler<DeleteSchoolClassCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeleteSchoolClassCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var rowVersionBytes = Convert.FromBase64String(request.RowVersionBase64);

        var deletedState = await classRepository.DeleteAsync(
            request.ClassId,
            rowVersionBytes,
            cancellationToken).ConfigureAwait(false);

        if (deletedState is null)
        {
            return Error.Conflict(
                code: "SchoolClass.ConcurrencyOrNotFound",
                description: "Клас не знайдено, або його дані вже були змінені іншим користувачем. Будь ласка, оновіть сторінку.");
        }

        auditContext.TrackNewState(deletedState);

        return Result.Success;
    }
}