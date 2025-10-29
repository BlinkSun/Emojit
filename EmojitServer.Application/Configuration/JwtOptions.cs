using System;

namespace EmojitServer.Application.Configuration;

/// <summary>
/// Represents configuration values controlling JWT authentication behavior.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// The configuration section name used to bind <see cref="JwtOptions"/>.
    /// </summary>
    public const string SectionName = "Jwt";

    private const int MinimumSigningKeyLengthInBytes = 32;

    /// <summary>
    /// Gets or sets the token issuer value embedded inside issued JWT tokens.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the intended audience for issued JWT tokens.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the symmetric signing key used to protect issued tokens.
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the access token lifetime in minutes.
    /// </summary>
    public int AccessTokenLifetimeInMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets the allowed clock skew in seconds when validating token expiry.
    /// </summary>
    public int ClockSkewInSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets a value indicating whether HTTPS metadata is required for JWT validation endpoints.
    /// </summary>
    public bool RequireHttpsMetadata { get; set; } = true;

    /// <summary>
    /// Validates the configured options and throws when invalid values are detected.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the configuration is invalid.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Issuer))
        {
            throw new InvalidOperationException("A JWT issuer must be configured.");
        }

        if (string.IsNullOrWhiteSpace(Audience))
        {
            throw new InvalidOperationException("A JWT audience must be configured.");
        }

        if (string.IsNullOrWhiteSpace(SigningKey))
        {
            throw new InvalidOperationException("A JWT signing key must be configured.");
        }

        byte[] signingKeyBytes;
        try
        {
            signingKeyBytes = Convert.FromBase64String(SigningKey);
        }
        catch (FormatException)
        {
            signingKeyBytes = System.Text.Encoding.UTF8.GetBytes(SigningKey);
        }

        if (signingKeyBytes.Length < MinimumSigningKeyLengthInBytes)
        {
            throw new InvalidOperationException("The JWT signing key must be at least 256 bits (32 bytes).");
        }

        if (AccessTokenLifetimeInMinutes <= 0)
        {
            throw new InvalidOperationException("The JWT access token lifetime must be greater than zero minutes.");
        }

        if (ClockSkewInSeconds < 0)
        {
            throw new InvalidOperationException("The JWT clock skew must be zero or a positive number of seconds.");
        }
    }
}
