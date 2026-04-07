using System.Security.Cryptography;
using System.Text;

namespace IssueSense.Application.Security;

public static class PasswordSecurity
{
    private const string AlgorithmMarker = "PBKDF2";
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int IterationCount = 100_000;

    public static string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            IterationCount,
            HashAlgorithmName.SHA256,
            KeySize);

        return string.Join(
            '$',
            AlgorithmMarker,
            IterationCount,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public static PasswordVerificationResult VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
        {
            return PasswordVerificationResult.Failed;
        }

        if (IsLegacySha256Hash(storedHash))
        {
            return VerifyLegacySha256(password, storedHash)
                ? PasswordVerificationResult.SuccessRehashNeeded
                : PasswordVerificationResult.Failed;
        }

        var parts = storedHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4 || !string.Equals(parts[0], AlgorithmMarker, StringComparison.Ordinal))
        {
            return PasswordVerificationResult.Failed;
        }

        if (!int.TryParse(parts[1], out var iterations) || iterations < 10_000)
        {
            return PasswordVerificationResult.Failed;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expectedHash = Convert.FromBase64String(parts[3]);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash)
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            return PasswordVerificationResult.Failed;
        }
    }

    private static bool VerifyLegacySha256(string password, string storedHash)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        var computedHash = Convert.ToHexString(bytes);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(storedHash));
    }

    private static bool IsLegacySha256Hash(string hash) =>
        hash.Length == 64 && hash.All(Uri.IsHexDigit);
}

public enum PasswordVerificationResult
{
    Failed = 0,
    Success = 1,
    SuccessRehashNeeded = 2
}
