using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.GradeType.CreateGradeType;

public sealed record CreateGradeTypeCommand(string TypeName) : IRequest<ErrorOr<Guid>>;