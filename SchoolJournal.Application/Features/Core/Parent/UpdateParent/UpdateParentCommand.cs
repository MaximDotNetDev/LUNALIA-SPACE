using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.Parent.UpdateParent;

public sealed record UpdateParentCommand(
    Guid ParentId,
    string? LastName,
    string? FirstName,
    string? MiddleName,
    string? Phone,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;