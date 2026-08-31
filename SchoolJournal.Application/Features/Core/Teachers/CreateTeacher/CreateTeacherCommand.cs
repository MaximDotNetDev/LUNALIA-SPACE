using ErrorOr;
using MediatR;
using SchoolJournal.Domain.Enums;

namespace SchoolJournal.Application.Features.Core.Teachers.CreateTeacher;

public sealed record CreateTeacherCommand(
    string LastName,
    string FirstName,
    string? MiddleName,
    string? Phone,
    string? Specialization,
    DateTimeOffset? DateOfBirth,
    Gender Gender,
    decimal? Workload,
    string? EducationInfo,
    string? MeetLink,
    Guid PositionId,
    Guid QualificationId,
    Guid? PedagogicalTitleId,
    Guid? UserId
) : IRequest<ErrorOr<Guid>>;