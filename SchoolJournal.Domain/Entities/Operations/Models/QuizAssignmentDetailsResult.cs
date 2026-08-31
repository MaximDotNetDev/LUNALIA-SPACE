namespace SchoolJournal.Domain.Entities.Operations.Models;

public class QuizAssignmentDetailsResult
{
    public Guid AssignmentId { get; set; }
    public Guid QuizId { get; set; }
    public Guid ClassId { get; set; }
    public DateTimeOffset AssignedDate { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public IReadOnlyCollection<byte> RowVersion { get; set; } = [];
    public string QuizTitle { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
}