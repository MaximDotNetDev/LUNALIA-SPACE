using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.Student.TransferStudent;

public sealed record TransferStudentCommand(
    Guid StudentId,
    Guid NewClassId,
    string RowVersionBase64) : IRequest<ErrorOr<Success>>;