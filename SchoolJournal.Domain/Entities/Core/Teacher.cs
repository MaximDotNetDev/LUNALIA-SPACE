using SchoolJournal.Domain.Enums;

namespace SchoolJournal.Domain.Entities.Core;

public sealed record Teacher
{
    public Guid TeacherId { get; init; }

    public required string LastName { get; init; }

    public required string FirstName { get; init; }

    public string? MiddleName { get; init; }

    public string? Phone { get; init; }

    public string? Specialization { get; init; }

    public DateTimeOffset? DateOfBirth { get; init; }

    public Gender Gender { get; init; }

    public decimal? Workload { get; init; }

    public string? EducationInfo { get; init; }

    public string? MeetLink { get; init; }

    public Guid? UserId { get; init; }

    public Guid PositionId { get; init; }

    public Guid QualificationId { get; init; }

    public Guid? PedagogicalTitleId { get; init; }

    public bool IsActive { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];
}