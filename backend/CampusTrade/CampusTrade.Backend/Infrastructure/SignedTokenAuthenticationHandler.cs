using System.Security.Claims;
using System.Text.Encodings.Web;
using CampusTrade.Backend.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace CampusTrade.Backend.Infrastructure;

public sealed class SignedTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "SignedToken";

    private readonly ITokenService _tokenService;

    public SignedTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ITokenService tokenService)
        : base(options, logger, encoder)
    {
        _tokenService = tokenService;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorization = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorization))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        const string bearerPrefix = "Bearer ";
        var token = authorization.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase)
            ? authorization[bearerPrefix.Length..].Trim()
            : authorization.Trim();

        if (!_tokenService.TryValidateToken(token, out var userId))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid or expired access token."));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
