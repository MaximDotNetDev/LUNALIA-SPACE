namespace SchoolJournal.Contracts.DTOs.Identity.Login;

public sealed record LoginRequest(
    string Login,
    string Password,
    string? DeviceIdentifier = null);