using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.Qualifications;

namespace SchoolJournal.Application.Features.Reference.Qualification.GetDeletedQualifications;

public sealed record GetDeletedQualificationsQuery(
    PageRequest PageRequest
) : IRequest<PagedResponse<QualificationResponse>>;