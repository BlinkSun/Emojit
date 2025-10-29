using System;
using System.Collections.Generic;

namespace EmojitServer.Api.Models.Realtime;

/// <summary>
/// Represents the payload broadcast when a new round starts.
/// </summary>
public sealed class RoundStartEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoundStartEvent"/> class.
    /// </summary>
    /// <param name="gameId">The identifier of the game session.</param>
    /// <param name="roundNumber">The one-based round number.</param>
    /// <param name="sharedCardIndex">The index of the shared card visible to all players.</param>
    /// <param name="playerCardIndexes">The mapping of player identifiers to their respective card indexes.</param>
    /// <param name="startedAtUtc">The timestamp when the round started in UTC.</param>
    public RoundStartEvent(
        string gameId,
        int roundNumber,
        int sharedCardIndex,
        IReadOnlyDictionary<string, int> playerCardIndexes,
        DateTimeOffset startedAtUtc)
    {
        GameId = gameId;
        RoundNumber = roundNumber;
        SharedCardIndex = sharedCardIndex;
        PlayerCardIndexes = playerCardIndexes;
        StartedAtUtc = startedAtUtc;
    }

    /// <summary>
    /// Gets the identifier of the game session.
    /// </summary>
    public string GameId { get; }

    /// <summary>
    /// Gets the one-based round number.
    /// </summary>
    public int RoundNumber { get; }

    /// <summary>
    /// Gets the index of the shared card visible to all players.
    /// </summary>
    public int SharedCardIndex { get; }

    /// <summary>
    /// Gets the mapping of player identifiers to their current card indexes.
    /// </summary>
    public IReadOnlyDictionary<string, int> PlayerCardIndexes { get; }

    /// <summary>
    /// Gets the timestamp in UTC when the round started.
    /// </summary>
    public DateTimeOffset StartedAtUtc { get; }
}
