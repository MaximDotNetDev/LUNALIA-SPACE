using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.SchoolClasses.DeleteSchoolClass;

public sealed record DeleteSchoolClassCommand(
    Guid ClassId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;