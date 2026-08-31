using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.Grade.CreateGrade;

public sealed record CreateGradeCommand(
    Guid LessonId,
    Guid StudentId,
    string GradeValue,
    string? Comment,
    Guid GradeTypeId
) : IRequest<ErrorOr<Guid>>;