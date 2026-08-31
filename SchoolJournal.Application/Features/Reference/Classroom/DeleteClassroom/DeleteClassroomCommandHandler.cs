using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.Classroom.DeleteClassroom;

public sealed class DeleteClassroomCommandHandler(
    IClassroomRepository classroomRepository,
    IAuditContext auditContext)
    : IRequestHandler<DeleteClassroomCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeleteClassroomCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        byte[] rowVersionBytes = Convert.FromBase64String(request.RowVersionBase64);

        var oldState = await classroomRepository.DeleteAsync(request.RoomId, rowVersionBytes, cancellationToken).ConfigureAwait(false);

        if (oldState is null)
        {
            return Error.Conflict(
                code: "Classroom.ConcurrencyConflict",
                description: "Аудиторію не знайдено, або її було змінено/видалено іншим користувачем. Оновіть сторінку та спробуйте ще раз.");
        }

        auditContext.TrackOldState(oldState);

        return Result.Success;
    }
}