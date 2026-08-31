namespace SchoolJournal.Domain.Entities.Operations.Models;

public sealed record TeacherSubstitutionDetailed
{
    public Guid SubstitutionId { get; init; }
    public Guid AssignmentId { get; init; }
    public Guid SubstituteTeacherId { get; init; }
    public string SubstituteTeacherFullName { get; init; } = string.Empty;
    public string MainTeacherFullName { get; init; } = string.Empty;
    public string SubjectName { get; init; } = string.Empty;
    public string ClassName { get; init; } = string.Empty;
    public string? SubgroupName { get; init; }
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset EndDate { get; init; }
    public bool IsDeleted { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];
}