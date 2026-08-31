using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Domain.Entities.Identity;

public sealed record Role
{
    public Guid RoleId { get; init; }

    public required RoleType RoleName { get; init; }

    public string? Description { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

}