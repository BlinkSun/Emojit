using System;

namespace EmojitServer.Api.Models.Authentication;

/// <summary>
/// Represents the response containing a freshly issued authentication token.
/// </summary>
public sealed class TokenResponse
{
    /// <summary>
    /// Gets or sets the serialized JWT token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of token returned.
    /// </summary>
    public string TokenType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expiration timestamp of the token in UTC.
    /// </summary>
    public DateTimeOffset ExpiresAtUtc { get; set; }
        = DateTimeOffset.UtcNow;
}
