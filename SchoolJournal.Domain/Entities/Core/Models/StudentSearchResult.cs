namespace SchoolJournal.Domain.Entities.Core.Models;

public class StudentSearchResult
{
    public Guid StudentId { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public Guid ClassId { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? UserId { get; set; }
    public string? Login { get; set; } // Логін з таблиці Users
}