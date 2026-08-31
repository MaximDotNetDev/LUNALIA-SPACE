using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.Lessons;

namespace SchoolJournal.Application.Features.Operations.Lessons.GetLessonsByAssignment;

public sealed record GetLessonsByAssignmentQuery(Guid AssignmentId) : IRequest<ErrorOr<IReadOnlyCollection<LessonResponse>>>;