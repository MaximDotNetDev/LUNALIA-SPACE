using ErrorOr;
using MediatR;
using SchoolJournal.Domain.Enums;

namespace SchoolJournal.Application.Features.Core.Teachers.UpdateTeacherProfile;

public sealed record UpdateTeacherProfileCommand(
    Guid TeacherId,
    string LastName,
    string FirstName,
    string? MiddleName,
    string? Phone,
    string? Specialization,
    DateTimeOffset? DateOfBirth,
    Gender Gender,
    string? EducationInfo,
    string? MeetLink,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;