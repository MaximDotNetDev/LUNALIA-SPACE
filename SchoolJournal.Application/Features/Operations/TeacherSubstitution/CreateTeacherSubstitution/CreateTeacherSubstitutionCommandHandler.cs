using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations.IRepositories;

namespace SchoolJournal.Application.Features.Operations.TeacherSubstitution.CreateTeacherSubstitution;

public sealed class CreateTeacherSubstitutionCommandHandler(
    ITeacherSubstitutionRepository teacherSubstitutionRepository,
    IAuditContext auditContext)
    : IRequestHandler<CreateTeacherSubstitutionCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateTeacherSubstitutionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await teacherSubstitutionRepository.HasOverlappingForAssignmentAsync(request.AssignmentId, request.StartDate, request.EndDate, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "TeacherSubstitution.AssignmentOverlap",
                description: "Для цього навчального призначення вже існує активна заміна на вказаний період.");
        }

        if (await teacherSubstitutionRepository.HasOverlappingForTeacherAsync(request.SubstituteTeacherId, request.StartDate, request.EndDate, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "TeacherSubstitution.TeacherOverlap",
                description: "Обраний вчитель вже призначений на іншу заміну в цей самий період.");
        }

        var substitution = new Domain.Entities.Operations.TeacherSubstitution
        {
            AssignmentId = request.AssignmentId,
            SubstituteTeacherId = request.SubstituteTeacherId,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        var substitutionId = await teacherSubstitutionRepository.AddAsync(substitution, cancellationToken).ConfigureAwait(false);

        var newState = await teacherSubstitutionRepository.GetByIdAsync(substitutionId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return substitutionId;
    }
}