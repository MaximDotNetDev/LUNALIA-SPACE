using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.Subject.DeleteSubject;

public sealed record DeleteSubjectCommand(
    Guid SubjectId
) : IRequest<ErrorOr<Success>>;