namespace SchoolJournal.Domain.Entities.Infrastructure;

public sealed record SystemSetting
{
    public Guid SettingId { get; init; }

    public int SettingKey { get; init; }

    public required string SchoolName { get; init; }

    public required string AcademicYear { get; init; }
     
    public string? PrincipalName { get; init; }

    public Guid UpdatedByUserId { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];
}