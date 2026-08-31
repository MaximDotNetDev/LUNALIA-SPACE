using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Parents;

namespace SchoolJournal.Application.Features.Core.Parent.GetParentById;

public sealed record GetParentByIdQuery(Guid ParentId) : IRequest<ErrorOr<ParentResponse>>;