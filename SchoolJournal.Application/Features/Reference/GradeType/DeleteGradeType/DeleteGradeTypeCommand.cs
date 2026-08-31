using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.GradeType.DeleteGradeType;

public sealed record DeleteGradeTypeCommand(Guid GradeTypeId) : IRequest<ErrorOr<Deleted>>;