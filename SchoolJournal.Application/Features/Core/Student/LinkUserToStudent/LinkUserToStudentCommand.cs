using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.Student.LinkUserToStudent;

public sealed record LinkUserToStudentCommand(
    Guid StudentId,
    Guid UserId,
    string RowVersionBase64) : IRequest<ErrorOr<Success>>;