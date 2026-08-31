using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.Classroom.UpdateClassroom;

public sealed class UpdateClassroomCommandHandler(
    IClassroomRepository classroomRepository,
    IAuditContext auditContext)
    : IRequestHandler<UpdateClassroomCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateClassroomCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await classroomRepository.ExistsByRoomNumberExcludingIdAsync(request.RoomNumber, request.RoomId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "Classroom.RoomNumberConflict",
                description: $"Інша аудиторія з номером '{request.RoomNumber}' вже існує.");
        }

        byte[] rowVersionBytes = Convert.FromBase64String(request.RowVersionBase64);

        var classroom = new Domain.Entities.Reference.Classroom
        {
            RoomId = request.RoomId,
            RoomNumber = request.RoomNumber,
            Name = request.Name,
            Capacity = request.Capacity,
            RowVersion = rowVersionBytes
        };

        var oldState = await classroomRepository.UpdateAsync(classroom, cancellationToken).ConfigureAwait(false);

        if (oldState is null)
        {
            return Error.Conflict(
                code: "Classroom.ConcurrencyConflict",
                description: "Аудиторія була змінена або видалена іншим користувачем. Оновіть сторінку та спробуйте ще раз.");
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