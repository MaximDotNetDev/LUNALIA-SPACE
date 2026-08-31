using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations.IRepositories;

namespace SchoolJournal.Application.Features.Operations.TeacherSubstitution.DeleteTeacherSubstitution;

public sealed class DeleteTeacherSubstitutionCommandHandler(
    ITeacherSubstitutionRepository teacherSubstitutionRepository,
    IAuditContext auditContext)
    : IRequestHandler<DeleteTeacherSubstitutionCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeleteTeacherSubstitutionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var oldState = await teacherSubstitutionRepository.GetByIdAsync(request.SubstitutionId, cancellationToken).ConfigureAwait(false);
        if (oldState is null || oldState.IsDeleted)
        {
            return Error.NotFound(
                code: "TeacherSubstitution.NotFound",
                description: "Заміну не знайдено або вона вже була видалена.");
        }

        auditContext.TrackOldState(oldState);

        var rowVersionBytes = Convert.FromBase64String(request.RowVersionBase64);
        var deletedState = await teacherSubstitutionRepository.DeleteAsync(request.SubstitutionId, rowVersionBytes, cancellationToken).ConfigureAwait(false);

        if (deletedState is null)
        {
            return Error.Conflict(
                code: "TeacherSubstitution.ConcurrencyConflict",
                description: "Запис був змінений іншим користувачем. Будь ласка, оновіть сторінку та спробуйте ще раз.");
        }

        auditContext.TrackNewState(deletedState);

        return Result.Success;
    }
}