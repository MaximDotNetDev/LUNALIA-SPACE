using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.Lessons.CreateLesson;

public sealed record CreateLessonCommand(
    Guid AssignmentId,
    DateTimeOffset LessonDate,
    string? LessonTopic,
    string? Homework,
    Guid LessonTypeId,
    Guid PeriodId,
    Guid RoomId,
    Guid SemesterId
) : IRequest<ErrorOr<Guid>>;