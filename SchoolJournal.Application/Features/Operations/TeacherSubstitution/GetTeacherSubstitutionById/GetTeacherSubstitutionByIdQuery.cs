using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.TeacherSubstitutions;

namespace SchoolJournal.Application.Features.Operations.TeacherSubstitution.GetTeacherSubstitutionById;

public sealed record GetTeacherSubstitutionByIdQuery(Guid SubstitutionId) : IRequest<ErrorOr<TeacherSubstitutionResponse>>;