using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.Student.DeleteStudent;

public sealed record DeleteStudentCommand(
    Guid StudentId,
    string RowVersionBase64) : IRequest<ErrorOr<Deleted>>;