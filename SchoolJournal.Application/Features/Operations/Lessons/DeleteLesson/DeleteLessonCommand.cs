using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.Lessons.DeleteLesson;

public sealed record DeleteLessonCommand(
    Guid LessonId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;