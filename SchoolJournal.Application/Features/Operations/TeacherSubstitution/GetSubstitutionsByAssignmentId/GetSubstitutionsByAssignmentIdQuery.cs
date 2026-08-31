using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.TeacherSubstitutions;

namespace SchoolJournal.Application.Features.Operations.TeacherSubstitution.GetSubstitutionsByAssignmentId;

public sealed record GetSubstitutionsByAssignmentIdQuery(Guid AssignmentId) : IRequest<ErrorOr<IEnumerable<TeacherSubstitutionResponse>>>;