using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.LessonTypes;

namespace SchoolJournal.Application.Features.Reference.LessonType.GetActiveLessonTypes;

public sealed record GetActiveLessonTypesQuery(PageRequest PageRequest) : IRequest<ErrorOr<PagedResponse<LessonTypeResponse>>>;