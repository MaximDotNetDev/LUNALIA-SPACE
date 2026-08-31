using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.PedagogicalTitles.CreatePedagogicalTitle;

public sealed record CreatePedagogicalTitleCommand(
    string TitleName
) : IRequest<ErrorOr<Guid>>;