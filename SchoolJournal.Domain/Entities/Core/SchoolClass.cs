namespace SchoolJournal.Domain.Entities.Core;

public sealed record SchoolClass
{
    public Guid ClassId { get; init; }

    public required string ClassName { get; init; }

    public int GradeLevel { get; init; }

    public required string AcademicYear { get; init; }

    public Guid HomeroomTeacherId { get; init; }

    public bool IsActive { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];
}