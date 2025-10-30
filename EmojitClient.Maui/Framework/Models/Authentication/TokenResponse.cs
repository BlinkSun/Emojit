namespace EmojitClient.Maui.Framework.Models.Authentication;

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
    /// Gets or sets the token type returned by the server.
    /// </summary>
    public string TokenType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expiration timestamp in UTC.
    /// </summary>
    public DateTimeOffset ExpiresAtUtc { get; set; }
        = DateTimeOffset.UtcNow;
}
