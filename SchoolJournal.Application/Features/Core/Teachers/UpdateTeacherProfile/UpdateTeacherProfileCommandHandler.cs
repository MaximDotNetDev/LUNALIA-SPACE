using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Teachers.UpdateTeacherProfile;

public sealed class UpdateTeacherProfileCommandHandler(
    ITeacherRepository teacherRepository,
    IAuditContext auditContext)
    : IRequestHandler<UpdateTeacherProfileCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateTeacherProfileCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. Перевірка на дублікат телефону (виключаючи поточного вчителя)
        if (!string.IsNullOrEmpty(request.Phone) &&
            await teacherRepository.ExistsByPhoneExcludingIdAsync(request.Phone, request.TeacherId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict("Teacher.DuplicatePhone", "Інший вчитель вже використовує цей номер телефону.");
        }

        // 2. Отримання поточного стану для аудиту та перевірки існування
        var currentTeacher = await teacherRepository.GetByIdAsync(request.TeacherId, cancellationToken).ConfigureAwait(false);
        if (currentTeacher is null)
        {
            return Error.NotFound("Teacher.NotFound", "Профіль вчителя не знайдено.");
        }

        auditContext.TrackOldState(currentTeacher);

        // 3. Підготовка сутності для оновлення
        var rowVersion = Convert.FromBase64String(request.RowVersionBase64);
        var updatedTeacher = currentTeacher with
        {
            LastName = request.LastName,
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            Phone = request.Phone,
            Specialization = request.Specialization,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            EducationInfo = request.EducationInfo,
            MeetLink = request.MeetLink,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = rowVersion
        };

        // 4. Виконання оновлення з перевіркою версії (Concurrency)
        var result = await teacherRepository.UpdateProfileAsync(updatedTeacher, cancellationToken).ConfigureAwait(false);

        if (result is null)
        {
            return Error.Conflict("Teacher.ConcurrencyConflict", "Дані були змінені іншим користувачем. Оновіть сторінку.");
        }

        auditContext.TrackNewState(result);

        return Result.Success;
    }
}