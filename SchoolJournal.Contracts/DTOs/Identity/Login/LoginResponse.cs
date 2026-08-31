using SchoolJournal.Contracts.Enums.Identity;

namespace SchoolJournal.Contracts.DTOs.Identity.Login;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds,
    RoleType Role);