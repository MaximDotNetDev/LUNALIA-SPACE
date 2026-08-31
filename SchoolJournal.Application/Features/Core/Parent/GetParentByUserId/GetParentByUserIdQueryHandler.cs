using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Parents;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Parent.GetParentByUserId;

public sealed class GetParentByUserIdQueryHandler(IParentRepository parentRepository)
    : IRequestHandler<GetParentByUserIdQuery, ErrorOr<ParentResponse>>
{
    public async Task<ErrorOr<ParentResponse>> Handle(GetParentByUserIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var parent = await parentRepository.GetByUserIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);

        if (parent is null)
        {
            return Error.NotFound(
                code: "Parent.ProfileNotFound",
                description: "До вашого облікового запису ще не прив'язано профіль батька.");
        }

        return parent.Adapt<ParentResponse>();
    }
}