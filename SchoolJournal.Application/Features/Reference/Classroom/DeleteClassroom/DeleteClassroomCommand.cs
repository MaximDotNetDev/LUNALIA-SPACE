using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.Classroom.DeleteClassroom;

public sealed record DeleteClassroomCommand(
    Guid RoomId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;