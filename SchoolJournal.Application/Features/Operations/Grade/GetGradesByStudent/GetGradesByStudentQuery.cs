using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.Grades;

namespace SchoolJournal.Application.Features.Operations.Grade.GetGradesByStudent;

public sealed record GetGradesByStudentQuery(
    Guid StudentId
) : IRequest<ErrorOr<IReadOnlyCollection<GradeResponse>>>;