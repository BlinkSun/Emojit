namespace EmojitClient.Maui.Models.Realtime;

/// <summary>
/// Represents the payload broadcast when a round attempt is processed.
/// </summary>
public sealed class RoundResultEvent
{
    /// <summary>
    /// Gets or sets the identifier of the game session associated with the result.
    /// </summary>
    public string GameId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the round resolved after the attempt.
    /// </summary>
    public bool RoundResolved { get; init; }
        = false;

    /// <summary>
    /// Gets or sets a value indicating whether the attempt was accepted by the server.
    /// </summary>
    public bool AttemptAccepted { get; init; }
        = false;

    /// <summary>
    /// Gets or sets the identifier of the player that resolved the round, when available.
    /// </summary>
    public string? ResolvingPlayerId { get; init; }
        = null;

    /// <summary>
    /// Gets or sets the resolving player's card index, when available.
    /// </summary>
    public int? ResolvingPlayerCardIndex { get; init; }
        = null;

    /// <summary>
    /// Gets or sets the identifier of the symbol that resolved the round, when available.
    /// </summary>
    public int? MatchingSymbolId { get; init; }
        = null;

    /// <summary>
    /// Gets or sets the round number affected by the attempt, when available.
    /// </summary>
    public int? RoundNumber { get; init; }
        = null;

    /// <summary>
    /// Gets or sets the timestamp when the attempt was processed in UTC.
    /// </summary>
    public DateTimeOffset ProcessedAtUtc { get; init; }
        = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the elapsed duration in milliseconds between the start of the round and its resolution, when available.
    /// </summary>
    public double? ResolutionDurationMilliseconds { get; init; }
        = null;

    /// <summary>
    /// Gets or sets the updated scoreboard snapshot, when available.
    /// </summary>
    public IReadOnlyDictionary<string, int>? Scores { get; init; }
        = null;

    /// <summary>
    /// Gets or sets a value indicating whether the game completed as a result of the attempt.
    /// </summary>
    public bool GameCompleted { get; init; }
        = false;
}
