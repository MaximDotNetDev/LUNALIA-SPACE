using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.SchoolClasses.ActivateSchoolClass;

public sealed record ActivateSchoolClassCommand(
    Guid ClassId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;