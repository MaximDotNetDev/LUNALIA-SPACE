namespace SchoolJournal.Application.Features.Reference.PedagogicalTitles.RestorePedagogicalTitle;

using ErrorOr;
using MediatR;

public sealed record RestorePedagogicalTitleCommand(
    Guid TitleId
) : IRequest<ErrorOr<Success>>;