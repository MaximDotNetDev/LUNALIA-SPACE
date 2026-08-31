using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.Grade.UpdateGrade;

public sealed record UpdateGradeCommand(
    Guid GradeId,
    string GradeValue,
    string? Comment,
    Guid GradeTypeId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;