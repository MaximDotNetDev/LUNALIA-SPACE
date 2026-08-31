namespace SchoolJournal.Contracts.DTOs.Identity.UpdateAccount;

public sealed record UpdateAccountRequest(
    string Login,
    string? NewPassword
);