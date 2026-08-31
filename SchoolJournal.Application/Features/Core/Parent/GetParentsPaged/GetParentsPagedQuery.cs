using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Core.Parents;

namespace SchoolJournal.Application.Features.Core.Parent.GetParentsPaged;

public sealed record GetParentsPagedQuery(PageRequest Page) : IRequest<PagedResponse<ParentResponse>>;