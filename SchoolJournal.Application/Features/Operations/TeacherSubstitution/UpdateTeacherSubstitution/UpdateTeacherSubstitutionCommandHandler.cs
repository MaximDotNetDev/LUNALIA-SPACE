using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations.IRepositories;

namespace SchoolJournal.Application.Features.Operations.TeacherSubstitution.UpdateTeacherSubstitution;

public sealed class UpdateTeacherSubstitutionCommandHandler(
    ITeacherSubstitutionRepository teacherSubstitutionRepository,
    IAuditContext auditContext)
    : IRequestHandler<UpdateTeacherSubstitutionCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateTeacherSubstitutionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var oldState = await teacherSubstitutionRepository.GetByIdAsync(request.SubstitutionId, cancellationToken).ConfigureAwait(false);
        if (oldState is null || oldState.IsDeleted)
        {
            return Error.NotFound(
                code: "TeacherSubstitution.NotFound",
                description: "Заміну не знайдено або вона була видалена.");
        }

        auditContext.TrackOldState(oldState);

        if (await teacherSubstitutionRepository.HasOverlappingForAssignmentExcludingIdAsync(request.AssignmentId, request.StartDate, request.EndDate, request.SubstitutionId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "TeacherSubstitution.AssignmentOverlap",
                description: "Для цього навчального призначення вже існує активна заміна на вказаний період.");
        }

        if (await teacherSubstitutionRepository.HasOverlappingForTeacherExcludingIdAsync(request.SubstituteTeacherId, request.StartDate, request.EndDate, request.SubstitutionId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "TeacherSubstitution.TeacherOverlap",
                description: "Обраний вчитель вже призначений на іншу заміну в цей самий період.");
        }

        var substitutionToUpdate = new Domain.Entities.Operations.TeacherSubstitution
        {
            SubstitutionId = request.SubstitutionId,
            AssignmentId = request.AssignmentId,
            SubstituteTeacherId = request.SubstituteTeacherId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = Convert.FromBase64String(request.RowVersionBase64)
        };

        var updatedState = await teacherSubstitutionRepository.UpdateAsync(substitutionToUpdate, cancellationToken).ConfigureAwait(false);

        if (updatedState is null)
        {
            return Error.Conflict(
                code: "TeacherSubstitution.ConcurrencyConflict",
                description: "Запис був змінений іншим користувачем. Будь ласка, оновіть сторінку та спробуйте ще раз.");
        }

        auditContext.TrackNewState(updatedState);

        return Result.Success;
    }
}