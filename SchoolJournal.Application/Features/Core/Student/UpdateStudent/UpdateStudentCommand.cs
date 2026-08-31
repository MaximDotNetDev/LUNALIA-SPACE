using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.Student.UpdateStudent;

public sealed record UpdateStudentCommand(
    Guid StudentId,
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
    string RowVersionBase64) : IRequest<ErrorOr<Success>>;