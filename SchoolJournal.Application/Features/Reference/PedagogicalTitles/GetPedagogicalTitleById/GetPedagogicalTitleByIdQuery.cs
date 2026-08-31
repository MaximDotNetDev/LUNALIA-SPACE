using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Reference.PedagogicalTitles;

namespace SchoolJournal.Application.Features.Reference.PedagogicalTitles.GetPedagogicalTitleById;

public sealed record GetPedagogicalTitleByIdQuery(
    Guid TitleId
) : IRequest<ErrorOr<PedagogicalTitleResponse>>;