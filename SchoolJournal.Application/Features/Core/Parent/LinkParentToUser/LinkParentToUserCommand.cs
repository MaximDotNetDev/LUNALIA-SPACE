using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.Parent.LinkParentToUser;

public sealed record LinkParentToUserCommand(
    Guid ParentId,
    Guid UserId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;