using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Reference.PedagogicalTitles;

namespace SchoolJournal.Application.Features.Reference.PedagogicalTitles.GetActivePedagogicalTitles;

public sealed record GetActivePedagogicalTitlesQuery : IRequest<ErrorOr<IEnumerable<PedagogicalTitleResponse>>>;