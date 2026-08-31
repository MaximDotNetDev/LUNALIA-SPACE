using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.StudentParents.AssignParentToStudent;

public sealed class AssignParentToStudentCommandHandler(
    IStudentParentRepository studentParentRepository,
    IStudentRepository studentRepository,
    IParentRepository parentRepository,
    IAuditContext auditContext)
    : IRequestHandler<AssignParentToStudentCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(AssignParentToStudentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var student = await studentRepository.GetByIdAsync(request.StudentId, cancellationToken).ConfigureAwait(false);
        if (student is null)
        {
            return Error.NotFound(
                code: "Student.NotFound",
                description: $"Учня з ідентифікатором '{request.StudentId}' не знайдено.");
        }

        var parent = await parentRepository.GetByIdAsync(request.ParentId, cancellationToken).ConfigureAwait(false);
        if (parent is null)
        {
            return Error.NotFound(
                code: "Parent.NotFound",
                description: $"Батьків з ідентифікатором '{request.ParentId}' не знайдено.");
        }

        if (await studentParentRepository.ExistsAsync(request.StudentId, request.ParentId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "StudentParent.Conflict",
                description: "Ці батьки вже прив'язані до цього учня.");
        }

        var studentParent = new StudentParent
        {
            StudentId = request.StudentId,
            ParentId = request.ParentId,
            Role = request.Role
        };

        var studentParentId = await studentParentRepository.AddAsync(studentParent, cancellationToken).ConfigureAwait(false);

        var newState = await studentParentRepository.GetByIdAsync(studentParentId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return studentParentId;
    }
}