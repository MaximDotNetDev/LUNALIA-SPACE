using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Reference.GradeTypes;

namespace SchoolJournal.Application.Features.Reference.GradeType.GetGradeTypeById;

public sealed record GetGradeTypeByIdQuery(Guid GradeTypeId) : IRequest<ErrorOr<GradeTypeResponse>>;