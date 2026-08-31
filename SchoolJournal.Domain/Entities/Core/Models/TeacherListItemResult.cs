namespace SchoolJournal.Domain.Entities.Core.Models;

public sealed record TeacherListItemResult
{
    public Guid TeacherId { get; init; }
    public required string LastName { get; init; }
    public required string FirstName { get; init; }
    public string? MiddleName { get; init; }
    public string? Phone { get; init; }
    public Guid PositionId { get; init; }
    public required string PositionName { get; init; }
    public Guid QualificationId { get; init; }
    public required string QualificationName { get; init; }
    public bool IsActive { get; init; }
    public Guid? UserId { get; init; }
    public string? Login { get; init; }
}