using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.Subject.UpdateSubject;

public sealed record UpdateSubjectCommand(
    Guid SubjectId,
    string SubjectName
) : IRequest<ErrorOr<Success>>;