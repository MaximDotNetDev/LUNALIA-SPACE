using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.PedagogicalTitles;

namespace SchoolJournal.Application.Features.Reference.PedagogicalTitles.GetActivePagedPedagogicalTitles;

public sealed record GetActivePagedPedagogicalTitlesQuery(
    PageRequest PageRequest
) : IRequest<ErrorOr<PagedResponse<PedagogicalTitleResponse>>>;