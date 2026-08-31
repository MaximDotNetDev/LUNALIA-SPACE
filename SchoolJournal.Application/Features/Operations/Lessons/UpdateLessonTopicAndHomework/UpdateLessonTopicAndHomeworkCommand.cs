using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.Lessons.UpdateLessonTopicAndHomework;

public sealed record UpdateLessonTopicAndHomeworkCommand(
    Guid LessonId,
    string? LessonTopic,
    string? Homework,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;