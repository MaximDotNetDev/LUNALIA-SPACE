using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.Student.CreateStudent;

public sealed record CreateStudentCommand(
    string LastName,
    string FirstName,
    string? MiddleName,
    DateTimeOffset? DateOfBirth,
    Guid ClassId,
    string? Gender,
    string? DocumentType,
    string? DocumentSeries,
    string? DocumentNumber,
    DateTimeOffset? EnrollmentDate,
    string? EnrollmentReason,
    string? Address,
    string? MedicalNotes,
    Guid? UserId) : IRequest<ErrorOr<Guid>>;