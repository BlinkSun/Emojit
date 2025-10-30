namespace EmojitClient.Maui.Framework.Models.Authentication;

/// <summary>
/// Represents the payload required to request a player authentication token from the server.
/// </summary>
public sealed class TokenRequest
{
    /// <summary>
    /// Gets or sets the identifier of the player requesting the token.
    /// </summary>
    public string PlayerId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name expected to match the player's profile.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
}
