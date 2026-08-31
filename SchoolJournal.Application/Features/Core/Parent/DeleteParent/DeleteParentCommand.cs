using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.Parent.DeleteParent;

public sealed record DeleteParentCommand(
    Guid ParentId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;