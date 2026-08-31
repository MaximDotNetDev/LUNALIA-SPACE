using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Reference.Classrooms;

namespace SchoolJournal.Application.Features.Reference.Classroom.GetClassroomById;

public sealed record GetClassroomByIdQuery(Guid RoomId) : IRequest<ErrorOr<ClassroomResponse>>;