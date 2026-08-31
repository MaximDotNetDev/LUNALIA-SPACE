using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.Classroom.RestoreClassroom;

public sealed record RestoreClassroomCommand(
    Guid RoomId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;