using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.LessonTypes;

namespace SchoolJournal.Application.Features.Reference.LessonType.GetDeletedLessonTypes;

public sealed record GetDeletedLessonTypesQuery(PageRequest PageRequest) : IRequest<ErrorOr<PagedResponse<LessonTypeResponse>>>;