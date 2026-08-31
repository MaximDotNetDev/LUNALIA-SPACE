using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.LessonType.CreateLessonType;

public sealed record CreateLessonTypeCommand(string TypeName) : IRequest<ErrorOr<Guid>>;