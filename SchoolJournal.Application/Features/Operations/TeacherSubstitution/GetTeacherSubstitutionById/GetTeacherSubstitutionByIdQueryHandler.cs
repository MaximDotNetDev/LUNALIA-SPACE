using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.TeacherSubstitutions;
using SchoolJournal.Domain.Entities.Operations.IRepositories;

namespace SchoolJournal.Application.Features.Operations.TeacherSubstitution.GetTeacherSubstitutionById;

public sealed class GetTeacherSubstitutionByIdQueryHandler(ITeacherSubstitutionRepository teacherSubstitutionRepository)
    : IRequestHandler<GetTeacherSubstitutionByIdQuery, ErrorOr<TeacherSubstitutionResponse>>
{
    public async Task<ErrorOr<TeacherSubstitutionResponse>> Handle(GetTeacherSubstitutionByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var substitution = await teacherSubstitutionRepository.GetByIdAsync(request.SubstitutionId, cancellationToken).ConfigureAwait(false);

        if (substitution is null || substitution.IsDeleted)
        {
            return Error.NotFound(
                code: "TeacherSubstitution.NotFound",
                description: "Заміну не знайдено.");
        }

        var response = new TeacherSubstitutionResponse(
                    substitution.SubstitutionId,
                    substitution.AssignmentId,
                    substitution.SubstituteTeacherId,
                    substitution.SubstituteTeacherFullName,
                    substitution.MainTeacherFullName,
                    substitution.SubjectName,
                    substitution.ClassName,
                    substitution.SubgroupName,
                    substitution.StartDate,
                    substitution.EndDate,
                    Convert.ToBase64String(substitution.RowVersion.ToArray())
                );
        
        return response;
    }
}