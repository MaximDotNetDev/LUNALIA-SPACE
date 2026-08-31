using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.Qualifications;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.Qualification.GetActiveQualifications;

public sealed class GetActiveQualificationsQueryHandler(
    IQualificationRepository qualificationRepository)
    : IRequestHandler<GetActiveQualificationsQuery, PagedResponse<QualificationResponse>>
{
    public async Task<PagedResponse<QualificationResponse>> Handle(GetActiveQualificationsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (items, totalCount) = await qualificationRepository.GetActivePagedAsync(
            request.PageRequest.Skip,
            request.PageRequest.PageSize,
            cancellationToken).ConfigureAwait(false);

        var dtos = items.Select(q => new QualificationResponse(
            q.QualificationId,
            q.QualificationName,
            Convert.ToBase64String(q.RowVersion.ToArray())
        )).ToList();

        return new PagedResponse<QualificationResponse>(
            dtos,
            request.PageRequest.PageNumber,
            request.PageRequest.PageSize,
            totalCount);
    }
}