using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Parents;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Parent.GetParentById;

public sealed class GetParentByIdQueryHandler(IParentRepository parentRepository)
    : IRequestHandler<GetParentByIdQuery, ErrorOr<ParentResponse>>
{
    public async Task<ErrorOr<ParentResponse>> Handle(GetParentByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var parent = await parentRepository.GetByIdAsync(request.ParentId, cancellationToken).ConfigureAwait(false);

        if (parent is null || parent.IsDeleted)
        {
            return Error.NotFound(
                code: "Parent.NotFound",
                description: "Профіль батьків не знайдено.");
        }

        return new ParentResponse(
                    parent.ParentId,
                    parent.LastName,
                    parent.FirstName,
                    parent.MiddleName,
                    parent.Phone,
                    parent.UserId,
                    parent.IsActive,
                    parent.CreatedAt,
                    parent.UpdatedAt,
                    Convert.ToBase64String(parent.RowVersion.ToArray()),
                    null 
                );
    }
}