namespace SchoolJournal.Domain.Entities.Operations;

public sealed record TeachingAssignment
{
    public Guid AssignmentId { get; init; }

    public Guid TeacherId { get; init; }

    public Guid SubjectId { get; init; }

    public Guid ClassId { get; init; }

    public Guid? SubgroupId { get; init; }

    public bool IsActive { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];
}