using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.LessonType.DeleteLessonType;

public sealed record DeleteLessonTypeCommand(Guid LessonTypeId) : IRequest<ErrorOr<Success>>;