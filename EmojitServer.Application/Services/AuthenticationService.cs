using EmojitServer.Application.Abstractions.Repositories;
using EmojitServer.Application.Abstractions.Services;
using EmojitServer.Application.Configuration;
using EmojitServer.Application.Services.Models;
using EmojitServer.Domain.Entities;
using EmojitServer.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EmojitServer.Application.Services;

/// <summary>
/// Provides authentication capabilities for players by issuing JWT access tokens.
/// </summary>
public sealed class AuthenticationService : IAuthenticationService
{
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly JwtOptions _jwtOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
    /// </summary>
    /// <param name="playerRepository">The repository used to retrieve player profiles.</param>
    /// <param name="options">The JWT configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public AuthenticationService(
        IPlayerRepository playerRepository,
        IOptions<JwtOptions> options,
        ILogger<AuthenticationService> logger)
    {
        _playerRepository = playerRepository;
        _logger = logger;

        ArgumentNullException.ThrowIfNull(options);
        _jwtOptions = options.Value ?? throw new InvalidOperationException("JWT configuration is not available.");
        _jwtOptions.Validate();
    }

    /// <inheritdoc />
    public async Task<AuthenticationResult> AuthenticatePlayerAsync(
        PlayerId playerId,
        string displayName,
        CancellationToken cancellationToken)
    {
        if (playerId.IsEmpty)
        {
            throw new ArgumentException("A player identifier must be provided.", nameof(playerId));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("A display name must be provided.", nameof(displayName));
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            Player? player = await _playerRepository
                .GetByIdAsync(playerId, cancellationToken)
                .ConfigureAwait(false);

            if (player is null)
            {
                throw new InvalidOperationException("The requested player does not exist.");
            }

            string normalizedDisplayName = displayName.Trim();
            if (!player.DisplayName.Equals(normalizedDisplayName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("The provided display name does not match the registered profile.");
            }

            DateTimeOffset issuedAtUtc = DateTimeOffset.UtcNow;
            DateTimeOffset expiresAtUtc = issuedAtUtc.AddMinutes(_jwtOptions.AccessTokenLifetimeInMinutes);

            JwtSecurityToken token = CreateToken(player, issuedAtUtc, expiresAtUtc);
            JwtSecurityTokenHandler handler = new();
            string serializedToken = handler.WriteToken(token);

            player.Touch(issuedAtUtc);
            await _playerRepository.UpdateAsync(player, cancellationToken).ConfigureAwait(false);

            return new AuthenticationResult(serializedToken, expiresAtUtc);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to authenticate player {PlayerId}.", playerId);
            throw new InvalidOperationException("Failed to authenticate player due to an unexpected error.", exception);
        }
    }

    private JwtSecurityToken CreateToken(Player player, DateTimeOffset issuedAtUtc, DateTimeOffset expiresAtUtc)
    {
        List<Claim> claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, player.Id.Value.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, player.DisplayName),
            new Claim("playerId", player.Id.Value.ToString()),
            new Claim("displayName", player.DisplayName),
        ];

        byte[] signingKeyBytes = GetSigningKeyBytes();
        SymmetricSecurityKey signingKey = new(signingKeyBytes);
        SigningCredentials signingCredentials = new(signingKey, SecurityAlgorithms.HmacSha256);

        return new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: issuedAtUtc.UtcDateTime,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: signingCredentials);
    }

    private byte[] GetSigningKeyBytes()
    {
        try
        {
            return Convert.FromBase64String(_jwtOptions.SigningKey);
        }
        catch (FormatException)
        {
            return Encoding.UTF8.GetBytes(_jwtOptions.SigningKey);
        }
    }
}
