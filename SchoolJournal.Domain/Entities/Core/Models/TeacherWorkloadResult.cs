namespace SchoolJournal.Domain.Entities.Core.Models;

public sealed record TeacherWorkloadResult
{
    public Guid TeacherId { get; init; }
    public required string LastName { get; init; }
    public required string FirstName { get; init; }
    public string? MiddleName { get; init; }
    public required string PositionName { get; init; }
    public decimal Workload { get; init; }
    public bool IsActive { get; init; }
}