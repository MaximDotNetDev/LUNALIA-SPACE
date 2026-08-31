namespace SchoolJournal.Domain.Entities.Core.Models;

public sealed record StudentHistory
{
    public Guid StudentId { get; init; }
    public required string LastName { get; init; }
    public required string FirstName { get; init; }
    public string? MiddleName { get; init; }
    public Guid ClassId { get; init; }
    public bool IsActive { get; init; }
    public bool IsDeleted { get; init; }
    public DateTimeOffset ValidFrom { get; init; }
    public DateTimeOffset ValidTo { get; init; }
}