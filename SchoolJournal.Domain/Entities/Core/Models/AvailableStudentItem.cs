namespace SchoolJournal.Domain.Entities.Core.Models;

public sealed record AvailableStudentItem
{
    public Guid StudentId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Patronymic { get; init; }
}