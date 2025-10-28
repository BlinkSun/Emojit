using System;
using EmojitServer.Domain.ValueObjects;

namespace EmojitServer.Domain.Entities;

/// <summary>
/// Represents a persisted record of a completed round within a game session.
/// </summary>
public sealed class RoundLog
{
    private RoundLog()
    {
    }

    private RoundLog(
        Guid id,
        GameId gameId,
        int roundNumber,
        PlayerId? winningPlayerId,
        int towerCardIndex,
        int? winningPlayerCardIndex,
        int matchingSymbolId,
        DateTimeOffset loggedAtUtc,
        TimeSpan resolutionTime)
    {
        Id = id;
        GameId = gameId;
        RoundNumber = roundNumber;
        WinningPlayerId = winningPlayerId;
        TowerCardIndex = towerCardIndex;
        WinningPlayerCardIndex = winningPlayerCardIndex;
        MatchingSymbolId = matchingSymbolId;
        LoggedAtUtc = loggedAtUtc;
        ResolutionTime = resolutionTime;
    }

    /// <summary>
    /// Gets the unique identifier of the log entry.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the identifier of the game session to which the round belongs.
    /// </summary>
    public GameId GameId { get; private set; }

    /// <summary>
    /// Gets the one-based round number.
    /// </summary>
    public int RoundNumber { get; private set; }

    /// <summary>
    /// Gets the identifier of the player that won the round, if any.
    /// </summary>
    public PlayerId? WinningPlayerId { get; private set; }

    /// <summary>
    /// Gets the index of the tower card shown during the round.
    /// </summary>
    public int TowerCardIndex { get; private set; }

    /// <summary>
    /// Gets the index of the winning player's card, if a winner exists.
    /// </summary>
    public int? WinningPlayerCardIndex { get; private set; }

    /// <summary>
    /// Gets the identifier of the matching symbol that resolved the round.
    /// </summary>
    public int MatchingSymbolId { get; private set; }

    /// <summary>
    /// Gets the timestamp at which the round result was logged in UTC.
    /// </summary>
    public DateTimeOffset LoggedAtUtc { get; private set; }

    /// <summary>
    /// Gets the duration between the start of the round and the resolution.
    /// </summary>
    public TimeSpan ResolutionTime { get; private set; }

    /// <summary>
    /// Creates a new <see cref="RoundLog"/> instance.
    /// </summary>
    /// <param name="gameId">The identifier of the game session.</param>
    /// <param name="roundNumber">The one-based round number.</param>
    /// <param name="winningPlayerId">The identifier of the winning player, when available.</param>
    /// <param name="towerCardIndex">The tower card index for the round.</param>
    /// <param name="winningPlayerCardIndex">The winning player's card index when a winner exists.</param>
    /// <param name="matchingSymbolId">The identifier of the matching symbol.</param>
    /// <param name="loggedAtUtc">The timestamp in UTC when the result was logged.</param>
    /// <param name="resolutionTime">The duration between the start of the round and its resolution.</param>
    /// <returns>A new <see cref="RoundLog"/>.</returns>
    public static RoundLog Create(
        GameId gameId,
        int roundNumber,
        PlayerId? winningPlayerId,
        int towerCardIndex,
        int? winningPlayerCardIndex,
        int matchingSymbolId,
        DateTimeOffset loggedAtUtc,
        TimeSpan resolutionTime)
    {
        if (gameId.IsEmpty)
        {
            throw new ArgumentException("Game identifier must be defined.", nameof(gameId));
        }

        if (roundNumber <= 0)
        {
            throw new ArgumentException("Round number must be greater than zero.", nameof(roundNumber));
        }

        if (towerCardIndex < 0)
        {
            throw new ArgumentException("Tower card index cannot be negative.", nameof(towerCardIndex));
        }

        if (winningPlayerCardIndex.HasValue && winningPlayerCardIndex.Value < 0)
        {
            throw new ArgumentException("Winning player card index cannot be negative.", nameof(winningPlayerCardIndex));
        }

        if (matchingSymbolId < 0)
        {
            throw new ArgumentException("Matching symbol identifier cannot be negative.", nameof(matchingSymbolId));
        }

        if (resolutionTime < TimeSpan.Zero)
        {
            throw new ArgumentException("Resolution time cannot be negative.", nameof(resolutionTime));
        }

        if (winningPlayerId.HasValue && winningPlayerId.Value.IsEmpty)
        {
            throw new ArgumentException("Winning player identifier must be defined when provided.", nameof(winningPlayerId));
        }

        DateTimeOffset normalizedTimestamp = EnsureUtc(loggedAtUtc);
        return new RoundLog(
            Guid.NewGuid(),
            gameId,
            roundNumber,
            winningPlayerId,
            towerCardIndex,
            winningPlayerCardIndex,
            matchingSymbolId,
            normalizedTimestamp,
            resolutionTime);
    }

    /// <summary>
    /// Updates the log to reflect an adjusted winner.
    /// </summary>
    /// <param name="winningPlayerId">The identifier of the corrected winner.</param>
    /// <param name="winningPlayerCardIndex">The card index associated with the winner.</param>
    /// <param name="matchingSymbolId">The matching symbol identifier.</param>
    /// <param name="loggedAtUtc">The timestamp when the adjustment was registered.</param>
    public void OverrideWinner(PlayerId winningPlayerId, int winningPlayerCardIndex, int matchingSymbolId, DateTimeOffset loggedAtUtc)
    {
        if (winningPlayerId.IsEmpty)
        {
            throw new ArgumentException("Winning player identifier must be defined.", nameof(winningPlayerId));
        }

        if (winningPlayerCardIndex < 0)
        {
            throw new ArgumentException("Winning player card index cannot be negative.", nameof(winningPlayerCardIndex));
        }

        if (matchingSymbolId < 0)
        {
            throw new ArgumentException("Matching symbol identifier cannot be negative.", nameof(matchingSymbolId));
        }

        WinningPlayerId = winningPlayerId;
        WinningPlayerCardIndex = winningPlayerCardIndex;
        MatchingSymbolId = matchingSymbolId;
        LoggedAtUtc = EnsureUtc(loggedAtUtc);
    }

    /// <summary>
    /// Updates the measured resolution time of the round.
    /// </summary>
    /// <param name="resolutionTime">The corrected resolution duration.</param>
    public void UpdateResolutionTime(TimeSpan resolutionTime)
    {
        if (resolutionTime < TimeSpan.Zero)
        {
            throw new ArgumentException("Resolution time cannot be negative.", nameof(resolutionTime));
        }

        ResolutionTime = resolutionTime;
    }

    private static DateTimeOffset EnsureUtc(DateTimeOffset timestamp)
    {
        return timestamp.Offset == TimeSpan.Zero
            ? timestamp
            : timestamp.ToUniversalTime();
    }
}
