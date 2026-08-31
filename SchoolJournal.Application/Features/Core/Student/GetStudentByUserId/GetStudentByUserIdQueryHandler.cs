using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Students;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Student.GetStudentByUserId;

public sealed class GetStudentByUserIdQueryHandler(IStudentRepository studentRepository)
    : IRequestHandler<GetStudentByUserIdQuery, ErrorOr<StudentResponse>>
{
    public async Task<ErrorOr<StudentResponse>> Handle(GetStudentByUserIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Звертаємося до методу, який ми додали в StudentRepository на попередньому етапі
        var student = await studentRepository.GetByUserIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);

        if (student is null)
        {
            return Error.NotFound("Student.NotFound", $"Профіль учня для користувача з ID '{request.UserId}' не знайдено.");
        }

        // Повертаємо DTO
        return new StudentResponse(
            student.StudentId,
            student.LastName,
            student.FirstName,
            student.MiddleName,
            student.DateOfBirth,
            student.ClassId,
            student.Gender.ToString(),
            student.DocumentType,
            student.DocumentSeries,
            student.DocumentNumber,
            student.EnrollmentDate,
            student.EnrollmentReason,
            student.Address,
            student.MedicalNotes,
            student.UserId,
            student.IsActive,
            Convert.ToBase64String(student.RowVersion.ToArray())
        );
    }
}