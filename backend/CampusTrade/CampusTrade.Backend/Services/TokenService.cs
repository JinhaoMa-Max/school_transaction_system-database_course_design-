using System.Security.Cryptography;
using System.Text;

namespace CampusTrade.Backend.Services;

public interface ITokenService
{
    string GenerateToken(int userId);
    bool TryValidateToken(string token, out int userId);
}

public class SignedTokenService : ITokenService
{
    private const string DefaultSigningKey = "CampusTrade.LocalDevelopment.SigningKey.ChangeForProduction.2026";
    private readonly byte[] _signingKey;
    private readonly int _expirationMinutes;

    public SignedTokenService(IConfiguration configuration)
    {
        var signingKey = configuration["Auth:TokenSigningKey"];
        if (string.IsNullOrWhiteSpace(signingKey))
        {
            signingKey = DefaultSigningKey;
        }

        _signingKey = Encoding.UTF8.GetBytes(signingKey);
        _expirationMinutes = configuration.GetValue("Auth:TokenExpirationMinutes", 1440);
    }

    public string GenerateToken(int userId)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_expirationMinutes).ToUnixTimeSeconds();
        var payload = $"{userId}|{expiresAt}";
        var payloadPart = Base64UrlEncode(Encoding.UTF8.GetBytes(payload));
        var signaturePart = Base64UrlEncode(Sign(payloadPart));
        return $"{payloadPart}.{signaturePart}";
    }

    public bool TryValidateToken(string token, out int userId)
    {
        userId = 0;
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var parts = token.Split('.', 2);
        if (parts.Length != 2)
        {
            return false;
        }

        var expectedSignature = Sign(parts[0]);
        byte[] actualSignature;
        try
        {
            actualSignature = Base64UrlDecode(parts[1]);
        }
        catch (FormatException)
        {
            return false;
        }

        if (!CryptographicOperations.FixedTimeEquals(expectedSignature, actualSignature))
        {
            return false;
        }

        string payload;
        try
        {
            payload = Encoding.UTF8.GetString(Base64UrlDecode(parts[0]));
        }
        catch (FormatException)
        {
            return false;
        }

        var payloadParts = payload.Split('|', 2);
        if (payloadParts.Length != 2)
        {
            return false;
        }

        if (!int.TryParse(payloadParts[0], out userId))
        {
            return false;
        }

        if (!long.TryParse(payloadParts[1], out var expiresAt))
        {
            return false;
        }

        return DateTimeOffset.UtcNow.ToUnixTimeSeconds() <= expiresAt;
    }

    private byte[] Sign(string payloadPart)
    {
        using var hmac = new HMACSHA256(_signingKey);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadPart));
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');
        var padding = (4 - base64.Length % 4) % 4;
        return Convert.FromBase64String(base64 + new string('=', padding));
    }
}
