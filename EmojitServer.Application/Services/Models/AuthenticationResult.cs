using System;

namespace EmojitServer.Application.Services.Models;

/// <summary>
/// Represents the outcome of a successful authentication request.
/// </summary>
public sealed class AuthenticationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationResult"/> class.
    /// </summary>
    /// <param name="accessToken">The issued JWT access token.</param>
    /// <param name="expiresAtUtc">The timestamp in UTC when the token expires.</param>
    public AuthenticationResult(string accessToken, DateTimeOffset expiresAtUtc)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new ArgumentException("An access token must be provided.", nameof(accessToken));
        }

        AccessToken = accessToken;
        ExpiresAtUtc = expiresAtUtc;
    }

    /// <summary>
    /// Gets the issued JWT access token.
    /// </summary>
    public string AccessToken { get; }

    /// <summary>
    /// Gets the timestamp in UTC when the token expires.
    /// </summary>
    public DateTimeOffset ExpiresAtUtc { get; }
}
