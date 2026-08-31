namespace SchoolJournal.Contracts.DTOs.Identity.Refresh;

public sealed record RefreshTokenRequest(
    string RefreshToken,
    string? DeviceIdentifier);