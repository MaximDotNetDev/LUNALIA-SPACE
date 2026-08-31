using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Student.TransferStudent;

public sealed class TransferStudentCommandHandler(
    IStudentRepository studentRepository,
    IAuditContext auditContext) : IRequestHandler<TransferStudentCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(TransferStudentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingStudent = await studentRepository.GetByIdAsync(request.StudentId, cancellationToken).ConfigureAwait(false);
        if (existingStudent is null || existingStudent.IsDeleted)
        {
            return Error.NotFound("Student.NotFound", "Учня не знайдено.");
        }

        if (existingStudent.ClassId == request.NewClassId)
        {
            return Error.Conflict("Student.SameClass", "Учень вже перебуває у цьому класі.");
        }

        auditContext.TrackOldState(existingStudent);

        var rowVersion = Convert.FromBase64String(request.RowVersionBase64);
        var updatedStudent = await studentRepository.TransferToClassAsync(
            request.StudentId,
            request.NewClassId,
            rowVersion,
            cancellationToken).ConfigureAwait(false);

        if (updatedStudent is null)
        {
            return Error.Conflict("Student.ConcurrencyConflict", "Запис був змінений іншим користувачем або видалений.");
        }

        auditContext.TrackNewState(updatedStudent);

        return Result.Success;
    }
}