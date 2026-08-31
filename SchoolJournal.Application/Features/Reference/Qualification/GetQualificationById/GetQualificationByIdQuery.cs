using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Reference.Qualifications;

namespace SchoolJournal.Application.Features.Reference.Qualification.GetQualificationById;

public sealed record GetQualificationByIdQuery(
    Guid QualificationId
) : IRequest<ErrorOr<QualificationResponse>>;