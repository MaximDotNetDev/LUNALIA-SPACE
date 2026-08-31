using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Operations.TeachingAssignments;

namespace SchoolJournal.Application.Features.Operations.TeachingAssignments.GetTeachingAssignmentsByTeacherId;

public sealed record GetTeachingAssignmentsByTeacherIdQuery(
    Guid TeacherId,
    PageRequest Page
) : IRequest<PagedResponse<TeachingAssignmentResponse>>;