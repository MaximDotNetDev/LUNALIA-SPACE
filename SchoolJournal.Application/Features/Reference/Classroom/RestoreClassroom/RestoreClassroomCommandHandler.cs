using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.Classroom.RestoreClassroom;

public sealed class RestoreClassroomCommandHandler(
    IClassroomRepository classroomRepository,
    IAuditContext auditContext)
    : IRequestHandler<RestoreClassroomCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(RestoreClassroomCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        byte[] rowVersionBytes = Convert.FromBase64String(request.RowVersionBase64);

        var oldState = await classroomRepository.RestoreAsync(request.RoomId, rowVersionBytes, cancellationToken).ConfigureAwait(false);

        if (oldState is null)
        {
            return Error.Conflict(
                code: "Classroom.ConcurrencyConflict",
                description: "Аудиторію не знайдено (можливо, вона вже активна або змінена іншим користувачем). Оновіть сторінку та спробуйте ще раз.");
        }

        auditContext.TrackOldState(oldState);

        var newState = await classroomRepository.GetByIdAsync(request.RoomId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return Result.Success;
    }
}