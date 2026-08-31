using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Reference.LessonTypes;

namespace SchoolJournal.Application.Features.Reference.LessonType.GetLessonTypeById;

public sealed record GetLessonTypeByIdQuery(Guid LessonTypeId) : IRequest<ErrorOr<LessonTypeResponse>>;