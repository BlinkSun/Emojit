namespace EmojitClient.Maui.Models.Leaderboard;

/// <summary>
/// Represents a single entry in the global leaderboard.
/// </summary>
public sealed class LeaderboardEntry
{
    /// <summary>
    /// Gets or sets the unique identifier of the player.
    /// </summary>
    public Guid PlayerId { get; init; }
        = Guid.Empty;

    /// <summary>
    /// Gets or sets the cumulative number of points earned by the player.
    /// </summary>
    public int TotalPoints { get; init; }
        = 0;

    /// <summary>
    /// Gets or sets the total number of games played by the player.
    /// </summary>
    public int GamesPlayed { get; init; }
        = 0;

    /// <summary>
    /// Gets or sets the total number of games won by the player.
    /// </summary>
    public int GamesWon { get; init; }
        = 0;

    /// <summary>
    /// Gets or sets the timestamp in UTC when the entry was last updated.
    /// </summary>
    public DateTimeOffset LastUpdatedAtUtc { get; init; }
        = DateTimeOffset.UtcNow;
}
