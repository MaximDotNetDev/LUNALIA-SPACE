namespace SchoolJournal.Domain.Entities.Core.Models;

public sealed record ParentStudentDetail
{
    public Guid StudentParentId { get; init; }

    public string? Role { get; init; }

    public Student Student { get; init; } = null!;
}