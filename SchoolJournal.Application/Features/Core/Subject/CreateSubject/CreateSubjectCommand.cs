using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.Subject.CreateSubject;

public sealed record CreateSubjectCommand(
    string SubjectName
) : IRequest<ErrorOr<Guid>>;