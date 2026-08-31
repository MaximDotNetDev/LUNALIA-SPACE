using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Student.UpdateMedicalNotes;

public sealed class UpdateMedicalNotesCommandHandler(
    IStudentRepository studentRepository,
    IAuditContext auditContext) : IRequestHandler<UpdateMedicalNotesCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateMedicalNotesCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingStudent = await studentRepository.GetByIdAsync(request.StudentId, cancellationToken).ConfigureAwait(false);
        if (existingStudent is null || existingStudent.IsDeleted)
        {
            return Error.NotFound("Student.NotFound", "Учня не знайдено.");
        }

        auditContext.TrackOldState(existingStudent);

        var rowVersion = Convert.FromBase64String(request.RowVersionBase64);
        var updatedStudent = await studentRepository.UpdateMedicalNotesAsync(
            request.StudentId,
            request.MedicalNotes,
            rowVersion,
            cancellationToken).ConfigureAwait(false);

        if (updatedStudent is null)
        {
            return Error.Conflict("Student.ConcurrencyConflict", "Конфлікт версій: дані були змінені або видалені.");
        }

        auditContext.TrackNewState(updatedStudent);

        return Result.Success;
    }
}