using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.TeachingAssignments;

namespace SchoolJournal.Application.Features.Operations.TeachingAssignments.GetTeachingAssignmentById;

public sealed record GetTeachingAssignmentByIdQuery(Guid Id) : IRequest<ErrorOr<TeachingAssignmentResponse>>;