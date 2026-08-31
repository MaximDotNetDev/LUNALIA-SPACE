using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.Subject.RestoreSubject;

public sealed record RestoreSubjectCommand(
    Guid SubjectId
) : IRequest<ErrorOr<Success>>;