namespace EmojitClient.Maui.Models.Realtime;

/// <summary>
/// Represents the payload sent to schedule a new game session.
/// </summary>
public sealed class CreateGameRequest
{
    /// <summary>
    /// Gets or sets the gameplay mode requested for the session.
    /// </summary>
    public GameMode Mode { get; set; } = GameMode.Tower;

    /// <summary>
    /// Gets or sets the maximum number of players allowed in the session.
    /// </summary>
    public int MaxPlayers { get; set; } = 4;

    /// <summary>
    /// Gets or sets the maximum number of rounds to play before completing the match.
    /// </summary>
    public int MaxRounds { get; set; } = 10;
}
