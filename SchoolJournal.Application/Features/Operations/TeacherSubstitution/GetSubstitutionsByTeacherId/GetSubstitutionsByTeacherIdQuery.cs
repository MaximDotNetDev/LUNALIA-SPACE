using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.TeacherSubstitutions;

namespace SchoolJournal.Application.Features.Operations.TeacherSubstitution.GetSubstitutionsByTeacherId;

public sealed record GetSubstitutionsByTeacherIdQuery(Guid TeacherId) : IRequest<ErrorOr<IEnumerable<TeacherSubstitutionResponse>>>;