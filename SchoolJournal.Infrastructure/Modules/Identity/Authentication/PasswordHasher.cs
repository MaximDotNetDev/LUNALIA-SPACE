using Konscious.Security.Cryptography;
using SchoolJournal.Application.Features.Identity.Common.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace SchoolJournal.Infrastructure.Modules.Identity.Authentication;

public sealed class PasswordHasher : IPasswordHasher
{
    private const int DegreeOfParallelism = 1;
    private const int MemorySize = 65536; 
    private const int Iterations = 3;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySize,
            Iterations = Iterations,
            Salt = salt
        };

        byte[] hash = argon2.GetBytes(HashSize);

        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string hash, string password)
    {
        ArgumentNullException.ThrowIfNull(hash);
        ArgumentNullException.ThrowIfNull(password);

        var parts = hash.Split(':');
        if (parts.Length != 2)
        {
            return false;
        }

        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] expectedHash = Convert.FromBase64String(parts[1]);

        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySize,
            Iterations = Iterations,
            Salt = salt
        };

        byte[] actualHash = argon2.GetBytes(HashSize);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}