using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace CampusTrade.Backend.Services;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string storedPassword);
}

public class Sha256PasswordHasher : IPasswordHasher
{
    private static readonly Regex Sha256HexPattern = new("^[a-fA-F0-9]{64}$", RegexOptions.Compiled);

    public string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public bool Verify(string password, string storedPassword)
    {
        if (string.IsNullOrEmpty(storedPassword))
        {
            return false;
        }

        if (!Sha256HexPattern.IsMatch(storedPassword))
        {
            return string.Equals(password, storedPassword, StringComparison.Ordinal);
        }

        var expected = Convert.FromHexString(storedPassword);
        var actual = Convert.FromHexString(Hash(password));
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
