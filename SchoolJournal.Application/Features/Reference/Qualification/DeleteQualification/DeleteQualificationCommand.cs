using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.Qualification.DeleteQualification;

public sealed record DeleteQualificationCommand(
    Guid QualificationId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;