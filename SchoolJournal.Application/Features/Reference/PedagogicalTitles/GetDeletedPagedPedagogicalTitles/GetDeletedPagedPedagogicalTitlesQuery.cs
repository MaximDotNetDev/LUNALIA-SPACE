using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.PedagogicalTitles;

namespace SchoolJournal.Application.Features.Reference.PedagogicalTitles.GetDeletedPagedPedagogicalTitles;

public sealed record GetDeletedPagedPedagogicalTitlesQuery(
    PageRequest PageRequest
) : IRequest<ErrorOr<PagedResponse<PedagogicalTitleResponse>>>;