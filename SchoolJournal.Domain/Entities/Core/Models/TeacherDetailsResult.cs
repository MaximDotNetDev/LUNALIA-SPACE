namespace SchoolJournal.Domain.Entities.Core.Models;

public sealed record TeacherDetailsResult
{
    public Guid TeacherId { get; init; }
    public required string LastName { get; init; }
    public required string FirstName { get; init; }
    public string? MiddleName { get; init; }
    public string? Phone { get; init; }
    public string? Specialization { get; init; }
    public DateTimeOffset? DateOfBirth { get; init; }
    public required string Gender { get; init; }
    public decimal? Workload { get; init; }
    public string? EducationInfo { get; init; }
    public string? MeetLink { get; init; }
    public Guid? UserId { get; init; }
    public Guid PositionId { get; init; }
    public required string PositionName { get; init; }
    public Guid QualificationId { get; init; }
    public required string QualificationName { get; init; }
    public Guid? PedagogicalTitleId { get; init; }
    public string? PedagogicalTitleName { get; init; }
    public bool IsActive { get; init; }
    public bool IsDeleted { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public required IReadOnlyCollection<byte> RowVersion { get; init; }
}