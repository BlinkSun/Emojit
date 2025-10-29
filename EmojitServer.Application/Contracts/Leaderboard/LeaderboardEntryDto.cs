namespace EmojitServer.Application.Contracts.Leaderboard;

/// <summary>
/// Represents the payload returned for leaderboard entries.
/// </summary>
public sealed class LeaderboardEntryDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the player.
    /// </summary>
    public Guid PlayerId { get; init; }

    /// <summary>
    /// Gets or sets the cumulative number of points earned by the player.
    /// </summary>
    public int TotalPoints { get; init; }

    /// <summary>
    /// Gets or sets the total number of games played.
    /// </summary>
    public int GamesPlayed { get; init; }

    /// <summary>
    /// Gets or sets the total number of games won.
    /// </summary>
    public int GamesWon { get; init; }

    /// <summary>
    /// Gets or sets the timestamp in UTC when the entry was last updated.
    /// </summary>
    public DateTimeOffset LastUpdatedAtUtc { get; init; }
}
