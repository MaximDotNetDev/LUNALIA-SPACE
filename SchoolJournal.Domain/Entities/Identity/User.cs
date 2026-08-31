using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Domain.Entities.Identity;

public sealed record User
{
    public Guid UserId { get; init; }

    public required string Login { get; init; }

    public string? Email { get; init; }

    public required string PasswordHash { get; init; }

    public Guid RoleId { get; init; }

    public RoleType Role { get; init; }

    public DateTimeOffset? LastLoginUtc { get; init; }

    public int FailedLoginAttempts { get; init; }

    public DateTimeOffset? LockoutEndUtc { get; init; }

    public bool IsActive { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];
}