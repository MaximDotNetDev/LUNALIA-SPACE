using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.GradeType.UpdateGradeType;

public sealed record UpdateGradeTypeCommand(
    Guid GradeTypeId,
    string TypeName
) : IRequest<ErrorOr<Updated>>;