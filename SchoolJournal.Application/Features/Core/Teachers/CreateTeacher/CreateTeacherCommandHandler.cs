using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Teachers.CreateTeacher;

public sealed class CreateTeacherCommandHandler(
    ITeacherRepository teacherRepository,
    IAuditContext auditContext)
    : IRequestHandler<CreateTeacherCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateTeacherCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!string.IsNullOrEmpty(request.Phone) &&
            await teacherRepository.ExistsByPhoneAsync(request.Phone, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict("Teacher.DuplicatePhone", "Вчитель з таким номером телефону вже існує.");
        }

        var teacher = new Teacher
        {
            LastName = request.LastName,
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            Phone = request.Phone,
            Specialization = request.Specialization,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            Workload = request.Workload,
            EducationInfo = request.EducationInfo,
            MeetLink = request.MeetLink,
            PositionId = request.PositionId,
            QualificationId = request.QualificationId,
            PedagogicalTitleId = request.PedagogicalTitleId,
            UserId = request.UserId,
            IsActive = true
        };

        var teacherId = await teacherRepository.AddAsync(teacher, cancellationToken).ConfigureAwait(false);

        var newState = await teacherRepository.GetByIdAsync(teacherId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return teacherId;
    }
}