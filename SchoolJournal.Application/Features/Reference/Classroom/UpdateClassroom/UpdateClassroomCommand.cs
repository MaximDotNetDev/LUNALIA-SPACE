using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.Classroom.UpdateClassroom;

public sealed record UpdateClassroomCommand(
    Guid RoomId,
    string RoomNumber,
    string? Name,
    int Capacity,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;