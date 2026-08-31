using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.Grades;

namespace SchoolJournal.Application.Features.Operations.Grade.GetGradeById;

public sealed record GetGradeByIdQuery(
    Guid GradeId
) : IRequest<ErrorOr<GradeResponse>>;