using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.Grade.DeleteGrade;

public sealed record DeleteGradeCommand(
    Guid GradeId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;