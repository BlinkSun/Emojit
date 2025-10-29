using System;
using System.Collections.Generic;

namespace EmojitServer.Api.Models.Realtime;

/// <summary>
/// Represents the payload broadcast when a round attempt is processed.
/// </summary>
public sealed class RoundResultEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoundResultEvent"/> class.
    /// </summary>
    /// <param name="gameId">The identifier of the game session.</param>
    /// <param name="roundResolved">Indicates whether the round resolved after the attempt.</param>
    /// <param name="attemptAccepted">Indicates whether the attempt was accepted.</param>
    /// <param name="resolvingPlayerId">The identifier of the player that resolved the round, when available.</param>
    /// <param name="resolvingPlayerCardIndex">The resolving player's card index, when available.</param>
    /// <param name="matchingSymbolId">The identifier of the symbol that resolved the round, when available.</param>
    /// <param name="roundNumber">The round number affected by the attempt, when available.</param>
    /// <param name="processedAtUtc">The timestamp when the attempt was processed in UTC.</param>
    /// <param name="resolutionDurationMilliseconds">The elapsed duration in milliseconds between the start of the round and its resolution.</param>
    /// <param name="scores">The updated scoreboard snapshot, when available.</param>
    /// <param name="gameCompleted">Indicates whether the game completed as a result of the attempt.</param>
    public RoundResultEvent(
        string gameId,
        bool roundResolved,
        bool attemptAccepted,
        string? resolvingPlayerId,
        int? resolvingPlayerCardIndex,
        int? matchingSymbolId,
        int? roundNumber,
        DateTimeOffset processedAtUtc,
        double? resolutionDurationMilliseconds,
        IReadOnlyDictionary<string, int>? scores,
        bool gameCompleted)
    {
        GameId = gameId;
        RoundResolved = roundResolved;
        AttemptAccepted = attemptAccepted;
        ResolvingPlayerId = resolvingPlayerId;
        ResolvingPlayerCardIndex = resolvingPlayerCardIndex;
        MatchingSymbolId = matchingSymbolId;
        RoundNumber = roundNumber;
        ProcessedAtUtc = processedAtUtc;
        ResolutionDurationMilliseconds = resolutionDurationMilliseconds;
        Scores = scores;
        GameCompleted = gameCompleted;
    }

    /// <summary>
    /// Gets the identifier of the game session associated with the result.
    /// </summary>
    public string GameId { get; }

    /// <summary>
    /// Gets a value indicating whether the round resolved after the attempt.
    /// </summary>
    public bool RoundResolved { get; }

    /// <summary>
    /// Gets a value indicating whether the attempt was accepted by the server.
    /// </summary>
    public bool AttemptAccepted { get; }

    /// <summary>
    /// Gets the identifier of the player that resolved the round, when available.
    /// </summary>
    public string? ResolvingPlayerId { get; }

    /// <summary>
    /// Gets the resolving player's card index, when available.
    /// </summary>
    public int? ResolvingPlayerCardIndex { get; }

    /// <summary>
    /// Gets the identifier of the matching symbol used to resolve the round, when available.
    /// </summary>
    public int? MatchingSymbolId { get; }

    /// <summary>
    /// Gets the round number affected by the attempt, when available.
    /// </summary>
    public int? RoundNumber { get; }

    /// <summary>
    /// Gets the timestamp when the attempt was processed in UTC.
    /// </summary>
    public DateTimeOffset ProcessedAtUtc { get; }

    /// <summary>
    /// Gets the elapsed duration in milliseconds between the start of the round and its resolution, when available.
    /// </summary>
    public double? ResolutionDurationMilliseconds { get; }

    /// <summary>
    /// Gets the updated scoreboard snapshot, when available.
    /// </summary>
    public IReadOnlyDictionary<string, int>? Scores { get; }

    /// <summary>
    /// Gets a value indicating whether the game completed as a result of the attempt.
    /// </summary>
    public bool GameCompleted { get; }
}
