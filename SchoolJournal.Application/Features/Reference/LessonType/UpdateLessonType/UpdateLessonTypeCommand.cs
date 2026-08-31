using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.LessonType.UpdateLessonType;

public sealed record UpdateLessonTypeCommand(
    Guid LessonTypeId,
    string TypeName) : IRequest<ErrorOr<Success>>;