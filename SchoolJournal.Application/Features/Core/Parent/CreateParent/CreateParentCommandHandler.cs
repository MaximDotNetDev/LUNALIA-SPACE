using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Parent.CreateParent;

public sealed class CreateParentCommandHandler(
    IParentRepository parentRepository,
    IAuditContext auditContext)
    : IRequestHandler<CreateParentCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateParentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var parent = new Domain.Entities.Core.Parent
        {
            LastName = request.LastName,
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            Phone = request.Phone
        };

        var parentId = await parentRepository.AddAsync(parent, cancellationToken).ConfigureAwait(false);

        var newState = await parentRepository.GetByIdAsync(parentId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return parentId;
    }
}