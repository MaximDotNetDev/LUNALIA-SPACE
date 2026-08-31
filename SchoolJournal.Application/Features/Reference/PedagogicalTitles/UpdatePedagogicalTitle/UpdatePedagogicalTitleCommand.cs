using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.PedagogicalTitles.UpdatePedagogicalTitle;

public sealed record UpdatePedagogicalTitleCommand(
    Guid TitleId,
    string TitleName
) : IRequest<ErrorOr<Success>>;