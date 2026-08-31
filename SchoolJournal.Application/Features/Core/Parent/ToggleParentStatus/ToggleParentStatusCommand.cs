using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.Parent.ToggleParentStatus;

public sealed record ToggleParentStatusCommand(
    Guid ParentId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;