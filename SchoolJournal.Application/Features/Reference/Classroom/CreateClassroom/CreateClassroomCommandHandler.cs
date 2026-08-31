using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.Classroom.CreateClassroom;

public sealed class CreateClassroomCommandHandler(
    IClassroomRepository classroomRepository,
    IAuditContext auditContext)
    : IRequestHandler<CreateClassroomCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateClassroomCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await classroomRepository.ExistsByRoomNumberAsync(request.RoomNumber, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "Classroom.RoomNumberConflict",
                description: $"Аудиторія з номером '{request.RoomNumber}' вже існує.");
        }

        var classroom = new Domain.Entities.Reference.Classroom
        {
            RoomNumber = request.RoomNumber,
            Name = request.Name,
            Capacity = request.Capacity
        };

        var roomId = await classroomRepository.AddAsync(classroom, cancellationToken).ConfigureAwait(false);

        var newState = await classroomRepository.GetByIdAsync(roomId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return roomId;
    }
}