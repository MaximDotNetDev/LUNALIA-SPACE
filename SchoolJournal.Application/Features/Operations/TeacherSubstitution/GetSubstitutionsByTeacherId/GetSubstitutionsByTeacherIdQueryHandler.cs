using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.TeacherSubstitutions;
using SchoolJournal.Domain.Entities.Operations.IRepositories;

namespace SchoolJournal.Application.Features.Operations.TeacherSubstitution.GetSubstitutionsByTeacherId;

public sealed class GetSubstitutionsByTeacherIdQueryHandler(ITeacherSubstitutionRepository teacherSubstitutionRepository)
    : IRequestHandler<GetSubstitutionsByTeacherIdQuery, ErrorOr<IEnumerable<TeacherSubstitutionResponse>>>
{
    public async Task<ErrorOr<IEnumerable<TeacherSubstitutionResponse>>> Handle(GetSubstitutionsByTeacherIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var substitutions = await teacherSubstitutionRepository.GetByTeacherIdAsync(request.TeacherId, cancellationToken).ConfigureAwait(false);

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