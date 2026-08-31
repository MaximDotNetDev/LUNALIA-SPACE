namespace SchoolJournal.Domain.Entities.Identity;

public sealed record RefreshToken
{
    public Guid TokenId { get; init; }

    public Guid UserId { get; init; }

    public required string TokenHash { get; init; }

    public DateTimeOffset ExpiresAt { get; init; }

    public string? CreatedByIp { get; init; }

    public string? DeviceIdentifier { get; init; }

    public bool Revoked { get; init; }

    public DateTimeOffset? RevokedAt { get; init; }

    public string? ReplacedByTokenHash { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];

    public RefreshToken Revoke() => this with
    {
        Revoked = true,
        RevokedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };
}