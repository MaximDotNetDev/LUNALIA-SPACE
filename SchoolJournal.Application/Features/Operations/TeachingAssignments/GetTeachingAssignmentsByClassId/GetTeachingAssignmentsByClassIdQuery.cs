using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Operations.TeachingAssignments;

namespace SchoolJournal.Application.Features.Operations.TeachingAssignments.GetTeachingAssignmentsByClassId;

public sealed record GetTeachingAssignmentsByClassIdQuery(
    Guid ClassId,
    PageRequest Page
) : IRequest<PagedResponse<TeachingAssignmentResponse>>;