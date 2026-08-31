using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.SchoolClasses.DeactivateSchoolClass;

public sealed record DeactivateSchoolClassCommand(
    Guid ClassId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;