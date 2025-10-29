namespace EmojitClient.Maui.Models.Realtime;

/// <summary>
/// Represents the payload sent when attempting to join an existing game session.
/// </summary>
public sealed class JoinGameRequest
{
    /// <summary>
    /// Gets or sets the identifier of the game session to join.
    /// </summary>
    public string GameId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the player joining the session.
    /// </summary>
    public string PlayerId { get; set; } = string.Empty;
}
