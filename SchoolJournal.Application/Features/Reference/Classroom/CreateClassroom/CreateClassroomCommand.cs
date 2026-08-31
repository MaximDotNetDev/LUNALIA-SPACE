using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.Classroom.CreateClassroom;

public sealed record CreateClassroomCommand(
    string RoomNumber,
    string? Name,
    int Capacity
) : IRequest<ErrorOr<Guid>>;