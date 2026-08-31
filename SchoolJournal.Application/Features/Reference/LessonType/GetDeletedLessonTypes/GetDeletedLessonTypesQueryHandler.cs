using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.LessonTypes;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.LessonType.GetDeletedLessonTypes;

public sealed class GetDeletedLessonTypesQueryHandler(ILessonTypeRepository lessonTypeRepository)
    : IRequestHandler<GetDeletedLessonTypesQuery, ErrorOr<PagedResponse<LessonTypeResponse>>>
{
    public async Task<ErrorOr<PagedResponse<LessonTypeResponse>>> Handle(GetDeletedLessonTypesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (items, totalCount) = await lessonTypeRepository.GetDeletedPagedAsync(
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