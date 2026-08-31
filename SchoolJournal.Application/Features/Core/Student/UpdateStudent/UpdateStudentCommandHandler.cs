using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;
using SchoolJournal.Domain.Enums;

namespace SchoolJournal.Application.Features.Core.Student.UpdateStudent;

public sealed class UpdateStudentCommandHandler(
    IStudentRepository studentRepository,
    IAuditContext auditContext) : IRequestHandler<UpdateStudentCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingStudent = await studentRepository.GetByIdAsync(request.StudentId, cancellationToken).ConfigureAwait(false);
        if (existingStudent is null || existingStudent.IsDeleted)
        {
            return Error.NotFound("Student.NotFound", "Учня не знайдено.");
        }

        if (!string.IsNullOrEmpty(request.DocumentType) &&
            !string.IsNullOrEmpty(request.DocumentNumber) &&
            await studentRepository.ExistsByDocumentExcludingIdAsync(request.DocumentType, request.DocumentNumber, request.StudentId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict("Student.DuplicateDocument", "Інший учень з таким документом уже зареєстрований.");
        }

        auditContext.TrackOldState(existingStudent);

        var studentToUpdate = existingStudent with
        {
            LastName = request.LastName,
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            DateOfBirth = request.DateOfBirth,
            ClassId = request.ClassId,
            Gender = Enum.TryParse<Gender>(request.Gender, true, out var g) ? g : Gender.None,
            DocumentType = request.DocumentType,
            DocumentSeries = request.DocumentSeries,
            DocumentNumber = request.DocumentNumber,
            EnrollmentDate = request.EnrollmentDate,
            EnrollmentReason = request.EnrollmentReason,
            Address = request.Address,
            MedicalNotes = request.MedicalNotes,
            RowVersion = Convert.FromBase64String(request.RowVersionBase64)
        };

        var updatedStudent = await studentRepository.UpdateAsync(studentToUpdate, cancellationToken).ConfigureAwait(false);

        if (updatedStudent is null)
        {
            return Error.Conflict("Student.ConcurrencyConflict", "Запис був змінений іншим користувачем або видалений.");
        }

        auditContext.TrackNewState(updatedStudent);

        return Result.Success;
    }
}