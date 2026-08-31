using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.Qualification.UpdateQualification;

public sealed record UpdateQualificationCommand(
    Guid QualificationId,
    string QualificationName,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;