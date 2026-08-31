using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Parents;

namespace SchoolJournal.Application.Features.Core.Parent.GetParentByUserId;

public sealed record GetParentByUserIdQuery(Guid UserId) : IRequest<ErrorOr<ParentResponse>>;