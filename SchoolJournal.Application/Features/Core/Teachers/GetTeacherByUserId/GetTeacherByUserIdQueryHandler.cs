using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Teachers;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Teachers.GetTeacherByUserId;

public sealed class GetTeacherByUserIdQueryHandler(ITeacherRepository teacherRepository)
    : IRequestHandler<GetTeacherByUserIdQuery, ErrorOr<TeacherResponse>>
{
    public async Task<ErrorOr<TeacherResponse>> Handle(GetTeacherByUserIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var details = await teacherRepository.GetDetailsByUserIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);

        if (details is null)
        {
            return Error.NotFound("Teacher.NotFound", "Профіль вчителя з вказаним обліковим записом не знайдено.");
        }

        var response = new TeacherResponse(
            details.TeacherId,
            details.LastName,
            details.FirstName,
            details.MiddleName,
            details.Phone,
            details.Specialization,
            details.DateOfBirth,
            details.Gender,
            details.Workload,
            details.EducationInfo,
            details.MeetLink,
            details.UserId,
            details.PositionId,
            details.PositionName,
            details.QualificationId,
            details.QualificationName,
            details.PedagogicalTitleId,
            details.PedagogicalTitleName,
            details.IsActive,
            details.IsDeleted,
            details.CreatedAt,
            details.UpdatedAt,
            Convert.ToBase64String(details.RowVersion.ToArray())
        );

        return response;
    }
}