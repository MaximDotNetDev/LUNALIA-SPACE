namespace SchoolJournal.Domain.Entities.Operations;

public sealed record Grade
{
    public Guid GradeId { get; init; }

    public Guid LessonId { get; init; }

    public Guid StudentId { get; init; }

    public required string GradeValue { get; init; }

    public string? Comment { get; init; }

    public Guid CreatedByUserId { get; init; }

    public Guid UpdatedByUserId { get; init; }

    public Guid GradeTypeId { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];
}