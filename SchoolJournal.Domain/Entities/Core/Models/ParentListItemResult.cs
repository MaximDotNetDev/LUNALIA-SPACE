namespace SchoolJournal.Domain.Entities.Core.Models;

public class ParentListItemResult
{
    public Guid ParentId { get; set; }
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? Phone { get; set; }
    public Guid? UserId { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public IReadOnlyCollection<byte> RowVersion { get; set; } = [];
    public string? Login { get; set; } 
}