namespace SchoolJournal.Contracts.DTOs.Identity.Login;

public sealed record TokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds);