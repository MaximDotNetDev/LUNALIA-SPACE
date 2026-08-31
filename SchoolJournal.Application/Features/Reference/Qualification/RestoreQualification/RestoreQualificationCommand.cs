using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.Qualification.RestoreQualification;

public sealed record RestoreQualificationCommand(
    Guid QualificationId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;