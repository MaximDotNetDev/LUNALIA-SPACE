// SchoolJournal.Application/Features/Core/SchoolClasses/GetActiveSchoolClasses/GetActiveSchoolClassesQueryHandler.cs
using Mapster;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Core.SchoolClasses;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.SchoolClasses.GetActiveSchoolClasses;

public sealed class GetActiveSchoolClassesQueryHandler(
    ISchoolClassRepository classRepository)
    : IRequestHandler<GetActiveSchoolClassesQuery, PagedResponse<SchoolClassItemResponse>>
{
    public async Task<PagedResponse<SchoolClassItemResponse>> Handle(GetActiveSchoolClassesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (items, totalCount) = await classRepository.GetActivePagedAsync(
            request.PageRequest.Skip,
            request.PageRequest.PageSize,
            request.AcademicYear,
            cancellationToken).ConfigureAwait(false);

        var responseItems = items.Adapt<IEnumerable<SchoolClassItemResponse>>();

        return new PagedResponse<SchoolClassItemResponse>(
            responseItems,
            request.PageRequest.PageNumber,
            request.PageRequest.PageSize,
            totalCount);
    }
}