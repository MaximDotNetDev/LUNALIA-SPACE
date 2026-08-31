using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.GradeType.RestoreGradeType;

public sealed record RestoreGradeTypeCommand(Guid GradeTypeId) : IRequest<ErrorOr<Success>>;