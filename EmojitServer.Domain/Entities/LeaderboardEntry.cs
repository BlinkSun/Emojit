using System;
using EmojitServer.Domain.ValueObjects;

namespace EmojitServer.Domain.Entities;

/// <summary>
/// Represents the aggregate leaderboard metrics for a player.
/// </summary>
public sealed class LeaderboardEntry
{
    private LeaderboardEntry()
    {
    }

    private LeaderboardEntry(PlayerId playerId, int totalPoints, int gamesPlayed, int gamesWon, DateTimeOffset lastUpdatedAtUtc)
    {
        PlayerId = playerId;
        TotalPoints = totalPoints;
        GamesPlayed = gamesPlayed;
        GamesWon = gamesWon;
        LastUpdatedAtUtc = lastUpdatedAtUtc;
    }

    /// <summary>
    /// Gets the identifier of the player represented by the leaderboard entry.
    /// </summary>
    public PlayerId PlayerId { get; private set; }

    /// <summary>
    /// Gets the total number of points accumulated by the player across all sessions.
    /// </summary>
    public int TotalPoints { get; private set; }

    /// <summary>
    /// Gets the total number of games played by the player.
    /// </summary>
    public int GamesPlayed { get; private set; }

    /// <summary>
    /// Gets the total number of games won by the player.
    /// </summary>
    public int GamesWon { get; private set; }

    /// <summary>
    /// Gets the timestamp of the last update applied to the leaderboard entry in UTC.
    /// </summary>
    public DateTimeOffset LastUpdatedAtUtc { get; private set; }

    /// <summary>
    /// Creates a new <see cref="LeaderboardEntry"/> with initial values.
    /// </summary>
    /// <param name="playerId">The identifier of the player.</param>
    /// <param name="initialPoints">The starting number of points.</param>
    /// <param name="gamesPlayed">The number of games already played.</param>
    /// <param name="gamesWon">The number of games already won.</param>
    /// <param name="updatedAtUtc">The timestamp of the entry creation in UTC.</param>
    /// <returns>A newly initialized leaderboard entry.</returns>
    public static LeaderboardEntry Create(PlayerId playerId, int initialPoints, int gamesPlayed, int gamesWon, DateTimeOffset updatedAtUtc)
    {
        if (playerId.IsEmpty)
        {
            throw new ArgumentException("Player identifier must be defined.", nameof(playerId));
        }

        if (initialPoints < 0)
        {
            throw new ArgumentException("Initial points cannot be negative.", nameof(initialPoints));
        }

        if (gamesPlayed < 0)
        {
            throw new ArgumentException("Games played cannot be negative.", nameof(gamesPlayed));
        }

        if (gamesWon < 0 || gamesWon > gamesPlayed)
        {
            throw new ArgumentException("Games won must be within the range of games played.", nameof(gamesWon));
        }

        return new LeaderboardEntry(playerId, initialPoints, gamesPlayed, gamesWon, EnsureUtc(updatedAtUtc));
    }

    /// <summary>
    /// Applies a finished game result to the leaderboard entry.
    /// </summary>
    /// <param name="pointsEarned">The number of points earned from the game.</param>
    /// <param name="won">Indicates whether the player won the game.</param>
    /// <param name="timestampUtc">The UTC timestamp when the result was recorded.</param>
    public void ApplyMatchResult(int pointsEarned, bool won, DateTimeOffset timestampUtc)
    {
        if (pointsEarned < 0)
        {
            throw new ArgumentException("Points earned cannot be negative.", nameof(pointsEarned));
        }

        TotalPoints += pointsEarned;
        GamesPlayed++;

        if (won)
        {
            GamesWon++;
        }

        DateTimeOffset normalizedTimestamp = EnsureUtc(timestampUtc);
        if (normalizedTimestamp > LastUpdatedAtUtc)
        {
            LastUpdatedAtUtc = normalizedTimestamp;
        }
    }

    /// <summary>
    /// Applies a manual adjustment to the player's total points.
    /// </summary>
    /// <param name="pointsDelta">The signed delta to apply.</param>
    /// <param name="timestampUtc">The UTC timestamp associated with the adjustment.</param>
    public void AdjustPoints(int pointsDelta, DateTimeOffset timestampUtc)
    {
        int newTotal = TotalPoints + pointsDelta;
        if (newTotal < 0)
        {
            throw new InvalidOperationException("Total points cannot become negative.");
        }

        TotalPoints = newTotal;
        DateTimeOffset normalizedTimestamp = EnsureUtc(timestampUtc);
        if (normalizedTimestamp > LastUpdatedAtUtc)
        {
            LastUpdatedAtUtc = normalizedTimestamp;
        }
    }

    private static DateTimeOffset EnsureUtc(DateTimeOffset timestamp)
    {
        return timestamp.Offset == TimeSpan.Zero
            ? timestamp
            : timestamp.ToUniversalTime();
    }
}
