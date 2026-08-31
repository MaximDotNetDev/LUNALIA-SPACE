using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.Qualifications;

namespace SchoolJournal.Application.Features.Reference.Qualification.GetActiveQualifications;

public sealed record GetActiveQualificationsQuery(
    PageRequest PageRequest
) : IRequest<PagedResponse<QualificationResponse>>;