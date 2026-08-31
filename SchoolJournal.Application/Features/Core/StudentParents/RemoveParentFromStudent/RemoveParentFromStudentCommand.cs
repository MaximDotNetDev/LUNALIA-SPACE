using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.StudentParents.RemoveParentFromStudent;

public sealed record RemoveParentFromStudentCommand(
    Guid StudentParentId
) : IRequest<ErrorOr<Success>>;