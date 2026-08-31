using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.Lessons;

namespace SchoolJournal.Application.Features.Operations.Lessons.GetClassroomOccupancy;

public sealed record GetClassroomOccupancyQuery(
    Guid RoomId,
    DateTimeOffset LessonDate,
    Guid? PeriodId
) : IRequest<ErrorOr<IReadOnlyCollection<LessonResponse>>>;