using SchoolJournal.Contracts.Enums.Identity;

namespace SchoolJournal.Contracts.DTOs.Identity.Roles;

public sealed record RoleResponse(
    Guid RoleId,
    RoleType RoleName,
    string? Description
);