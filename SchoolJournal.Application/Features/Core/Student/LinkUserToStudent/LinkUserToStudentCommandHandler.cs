using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Student.LinkUserToStudent;

public sealed class LinkUserToStudentCommandHandler(
    IStudentRepository studentRepository,
    IAuditContext auditContext) : IRequestHandler<LinkUserToStudentCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(LinkUserToStudentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingStudent = await studentRepository.GetByIdAsync(request.StudentId, cancellationToken).ConfigureAwait(false);
        if (existingStudent is null || existingStudent.IsDeleted)
        {
            return Error.NotFound("Student.NotFound", "Учня не знайдено.");
        }

        if (await studentRepository.IsUserAlreadyLinkedAsync(request.UserId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict("Student.UserAlreadyLinked", "Цей користувач уже прив'язаний до іншого учня.");
        }

        auditContext.TrackOldState(existingStudent);

        var rowVersion = Convert.FromBase64String(request.RowVersionBase64);
        var updatedStudent = await studentRepository.LinkUserAsync(
            request.StudentId,
            request.UserId,
            rowVersion,
            cancellationToken).ConfigureAwait(false);

        if (updatedStudent is null)
        {
            return Error.Conflict("Student.ConcurrencyConflict", "Конфлікт версій або запис було видалено.");
        }

        auditContext.TrackNewState(updatedStudent);

        return Result.Success;
    }
}