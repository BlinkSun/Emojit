namespace EmojitClient.Maui.Models.Realtime;

/// <summary>
/// Represents the payload broadcast when a new round begins.
/// </summary>
public sealed class RoundStartEvent
{
    /// <summary>
    /// Gets or sets the identifier of the game session associated with the round.
    /// </summary>
    public string GameId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the one-based round number.
    /// </summary>
    public int RoundNumber { get; init; }
        = 0;

    /// <summary>
    /// Gets or sets the index of the shared card visible to all players.
    /// </summary>
    public int SharedCardIndex { get; init; }
        = 0;

    /// <summary>
    /// Gets or sets the mapping of player identifiers to their current card indexes.
    /// </summary>
    public IReadOnlyDictionary<string, int> PlayerCardIndexes { get; init; }
        = new Dictionary<string, int>();

    /// <summary>
    /// Gets or sets the timestamp in UTC when the round started.
    /// </summary>
    public DateTimeOffset StartedAtUtc { get; init; }
        = DateTimeOffset.UtcNow;
}
