using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.TeacherSubstitutions;
using SchoolJournal.Domain.Entities.Operations.IRepositories;

namespace SchoolJournal.Application.Features.Operations.TeacherSubstitution.GetSubstitutionsByAssignmentId;

public sealed class GetSubstitutionsByAssignmentIdQueryHandler(ITeacherSubstitutionRepository teacherSubstitutionRepository)
    : IRequestHandler<GetSubstitutionsByAssignmentIdQuery, ErrorOr<IEnumerable<TeacherSubstitutionResponse>>>
{
    public async Task<ErrorOr<IEnumerable<TeacherSubstitutionResponse>>> Handle(GetSubstitutionsByAssignmentIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var substitutions = await teacherSubstitutionRepository.GetByAssignmentIdAsync(request.AssignmentId, cancellationToken).ConfigureAwait(false);

        var response = substitutions.Select(s => new TeacherSubstitutionResponse(
                    s.SubstitutionId,
                    s.AssignmentId,
                    s.SubstituteTeacherId,
                    s.SubstituteTeacherFullName,
                    s.MainTeacherFullName,
                    s.SubjectName,
                    s.ClassName,
                    s.SubgroupName,
                    s.StartDate,
                    s.EndDate,
                    Convert.ToBase64String(s.RowVersion.ToArray())
                )).ToList();

        return response;
    }
}