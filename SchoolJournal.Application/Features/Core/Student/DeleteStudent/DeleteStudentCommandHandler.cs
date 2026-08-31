using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Student.DeleteStudent;

public sealed class DeleteStudentCommandHandler(
    IStudentRepository studentRepository,
    IAuditContext auditContext) : IRequestHandler<DeleteStudentCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existing = await studentRepository.GetByIdAsync(request.StudentId, cancellationToken).ConfigureAwait(false);
        if (existing is null || existing.IsDeleted)
        {
            return Error.NotFound("Student.NotFound", "Учня не знайдено.");
        }

        auditContext.TrackOldState(existing);

        var rowVersion = Convert.FromBase64String(request.RowVersionBase64);
        var deletedStudent = await studentRepository.DeleteAsync(request.StudentId, rowVersion, cancellationToken).ConfigureAwait(false);

        if (deletedStudent is null)
        {
            return Error.Conflict("Student.ConcurrencyConflict", "Конфлікт версій або запис уже видалено.");
        }

        auditContext.TrackNewState(deletedStudent);

        return Result.Deleted;
    }
}