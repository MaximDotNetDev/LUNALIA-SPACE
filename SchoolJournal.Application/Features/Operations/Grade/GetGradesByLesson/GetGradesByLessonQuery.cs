using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.Grades;

namespace SchoolJournal.Application.Features.Operations.Grade.GetGradesByLesson;

public sealed record GetGradesByLessonQuery(
    Guid LessonId
) : IRequest<ErrorOr<IReadOnlyCollection<GradeResponse>>>;