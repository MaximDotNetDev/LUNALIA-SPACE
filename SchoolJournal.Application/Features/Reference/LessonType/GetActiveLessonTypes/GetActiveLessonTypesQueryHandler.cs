using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.LessonTypes;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.LessonType.GetActiveLessonTypes;

public sealed class GetActiveLessonTypesQueryHandler(ILessonTypeRepository lessonTypeRepository)
    : IRequestHandler<GetActiveLessonTypesQuery, ErrorOr<PagedResponse<LessonTypeResponse>>>
{
    public async Task<ErrorOr<PagedResponse<LessonTypeResponse>>> Handle(GetActiveLessonTypesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (items, totalCount) = await lessonTypeRepository.GetActivePagedAsync(
            request.PageRequest.Skip,
            request.PageRequest.PageSize,
            cancellationToken).ConfigureAwait(false);

        var responseItems = items.Adapt<IEnumerable<LessonTypeResponse>>();

        return new PagedResponse<LessonTypeResponse>(
            responseItems,
            request.PageRequest.PageNumber,
            request.PageRequest.PageSize,
            totalCount);
    }
}