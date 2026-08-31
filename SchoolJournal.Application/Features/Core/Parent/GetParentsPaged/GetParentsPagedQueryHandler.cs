using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Core.Parents;
using SchoolJournal.Domain.Entities.Core.IRepositories;
using System;
using System.Linq;

namespace SchoolJournal.Application.Features.Core.Parent.GetParentsPaged;

public sealed class GetParentsPagedQueryHandler(IParentRepository parentRepository)
    : IRequestHandler<GetParentsPagedQuery, PagedResponse<ParentResponse>>
{
    public async Task<PagedResponse<ParentResponse>> Handle(GetParentsPagedQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (items, totalCount) = await parentRepository.GetActivePagedAsync(
            request.Page.Skip,
            request.Page.PageSize,
            cancellationToken).ConfigureAwait(false);

        // Ручний мапінг миттєво вирішує проблему з RowVersionBase64 та новим полем Login
        var responseItems = items.Select(p => new ParentResponse(
            p.ParentId,
            p.LastName,
            p.FirstName,
            p.MiddleName,
            p.Phone,
            p.UserId,
            p.IsActive,
            p.CreatedAt,
            p.UpdatedAt,
            Convert.ToBase64String(p.RowVersion.ToArray()),
            p.Login
        )).ToList();

        return new PagedResponse<ParentResponse>(
            responseItems,
            request.Page.PageNumber,
            request.Page.PageSize,
            totalCount);
    }
}