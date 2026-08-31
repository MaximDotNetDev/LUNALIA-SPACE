using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.Parent.CreateParent;

public sealed record CreateParentCommand(
    string? LastName,
    string? FirstName,
    string? MiddleName,
    string? Phone
) : IRequest<ErrorOr<Guid>>;