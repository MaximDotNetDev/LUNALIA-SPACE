namespace SchoolJournal.Domain.Entities.Operations;

public sealed record TeacherSubstitution
{
    public Guid SubstitutionId { get; init; }

    public Guid AssignmentId { get; init; }

    public Guid SubstituteTeacherId { get; init; }

    public DateTimeOffset StartDate { get; init; }

    public DateTimeOffset EndDate { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];
}