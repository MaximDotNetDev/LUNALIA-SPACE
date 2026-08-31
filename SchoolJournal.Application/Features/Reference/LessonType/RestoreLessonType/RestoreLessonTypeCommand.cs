using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.LessonType.RestoreLessonType;

public sealed record RestoreLessonTypeCommand(Guid LessonTypeId) : IRequest<ErrorOr<Success>>;