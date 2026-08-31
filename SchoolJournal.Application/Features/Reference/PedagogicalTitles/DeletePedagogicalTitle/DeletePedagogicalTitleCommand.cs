using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.PedagogicalTitles.DeletePedagogicalTitle;

public sealed record DeletePedagogicalTitleCommand(
    Guid TitleId
) : IRequest<ErrorOr<Success>>;