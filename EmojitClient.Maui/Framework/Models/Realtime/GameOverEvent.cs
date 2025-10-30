namespace EmojitClient.Maui.Framework.Models.Realtime;

/// <summary>
/// Represents the payload broadcast when a game session finishes.
/// </summary>
public sealed class GameOverEvent
{
    /// <summary>
    /// Gets or sets the identifier of the completed game session.
    /// </summary>
    public string GameId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the final scores of all participants.
    /// </summary>
    public IReadOnlyDictionary<string, int> FinalScores { get; init; }
        = new Dictionary<string, int>();

    /// <summary>
    /// Gets or sets the timestamp when the game completed in UTC.
    /// </summary>
    public DateTimeOffset CompletedAtUtc { get; init; }
        = DateTimeOffset.UtcNow;
}
