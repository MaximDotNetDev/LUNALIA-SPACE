namespace SchoolJournal.Domain.Entities.Core.Models;

public sealed record StudentParentDetail
{
    public Guid StudentParentId { get; init; }

    public string? Role { get; init; }

    public Parent Parent { get; init; } = null!;
}