using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.Qualification.CreateQualification;

public sealed record CreateQualificationCommand(
    string QualificationName
) : IRequest<ErrorOr<Guid>>;