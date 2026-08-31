using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.TeacherSubstitutions;

namespace SchoolJournal.Application.Features.Operations.TeacherSubstitution.GetActiveSubstitutions;

public sealed record GetActiveSubstitutionsQuery() : IRequest<ErrorOr<IEnumerable<TeacherSubstitutionResponse>>>;