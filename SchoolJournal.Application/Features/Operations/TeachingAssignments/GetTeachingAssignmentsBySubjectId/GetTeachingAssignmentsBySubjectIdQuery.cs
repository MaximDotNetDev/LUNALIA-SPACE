using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Operations.TeachingAssignments;

namespace SchoolJournal.Application.Features.Operations.TeachingAssignments.GetTeachingAssignmentsBySubjectId;

public sealed record GetTeachingAssignmentsBySubjectIdQuery(
    Guid SubjectId,
    PageRequest Page
) : IRequest<PagedResponse<TeachingAssignmentResponse>>;