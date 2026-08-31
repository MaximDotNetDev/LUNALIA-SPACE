using MediatR;
using SchoolJournal.Contracts.DTOs.Reference.GradeTypes;

namespace SchoolJournal.Application.Features.Reference.GradeType.GetActiveGradeTypes;

public sealed record GetActiveGradeTypesQuery() : IRequest<IEnumerable<GradeTypeResponse>>;