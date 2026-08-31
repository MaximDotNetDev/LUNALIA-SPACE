namespace SchoolJournal.Infrastructure.Modules.Identity.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required string Secret { get; init; }
    public int ExpirationInMinutes { get; init; }
}