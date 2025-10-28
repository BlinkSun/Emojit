using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using EmojitServer.Core.Design;
using EmojitServer.Domain.Entities;
using EmojitServer.Domain.Enums;
using EmojitServer.Domain.ValueObjects;

namespace EmojitServer.Core.GameModes;

/// <summary>
/// Defines the behavior required for a game mode to be orchestrated by the Emojit server.
/// </summary>
public interface IGameMode
{
    /// <summary>
    /// Gets the gameplay mode identifier implemented by this instance.
    /// </summary>
    GameMode Mode { get; }

    /// <summary>
    /// Gets a value indicating whether the match handled by the mode has finished.
    /// </summary>
    bool IsGameOver { get; }

    /// <summary>
    /// Gets the current round descriptor when a round is active.
    /// </summary>
    GameRoundState? CurrentRound { get; }

    /// <summary>
    /// Initializes the game mode with the session, participants, and deterministic deck design.
    /// </summary>
    /// <param name="session">The session metadata the mode instance should operate against.</param>
    /// <param name="participants">The ordered collection of players participating in the session.</param>
    /// <param name="design">The deterministic deck design used to provide cards.</param>
    /// <param name="configuration">Additional configuration knobs that influence deck flow and limits.</param>
    void Initialize(
        GameSession session,
        IReadOnlyCollection<PlayerId> participants,
        EmojitDesign design,
        GameModeConfiguration configuration);

    /// <summary>
    /// Starts the next round and returns its descriptor.
    /// </summary>
    /// <param name="startedAtUtc">The timestamp marking when the round starts in UTC.</param>
    /// <returns>The newly active round descriptor.</returns>
    GameRoundState StartNextRound(DateTimeOffset startedAtUtc);

    /// <summary>
    /// Registers a player's symbol selection attempt and returns the resulting round resolution status.
    /// </summary>
    /// <param name="playerId">The identifier of the player submitting the attempt.</param>
    /// <param name="symbolId">The symbol identifier chosen by the player.</param>
    /// <param name="occurredAtUtc">The timestamp when the attempt occurred in UTC.</param>
    /// <returns>A resolution result describing whether the attempt resolved the round.</returns>
    RoundResolutionResult RegisterAttempt(PlayerId playerId, int symbolId, DateTimeOffset occurredAtUtc);

    /// <summary>
    /// Retrieves the latest scoreboard snapshot for the running match.
    /// </summary>
    /// <returns>A snapshot of the current scores.</returns>
    ScoreSnapshot GetScoreSnapshot();
}

/// <summary>
/// Represents configuration options supplied to a game mode before starting a session.
/// </summary>
public sealed class GameModeConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameModeConfiguration"/> class.
    /// </summary>
    /// <param name="maxRounds">The maximum number of rounds to play before the session completes.</param>
    /// <param name="shuffleDeck">Indicates whether the deck should be shuffled before the first round.</param>
    /// <param name="randomSeed">An optional deterministic seed used when shuffling.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the provided numeric arguments are invalid.</exception>
    public GameModeConfiguration(int maxRounds, bool shuffleDeck = true, int? randomSeed = null)
    {
        if (maxRounds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxRounds), maxRounds, "Maximum rounds must be greater than zero.");
        }

        if (randomSeed.HasValue && randomSeed.Value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(randomSeed), randomSeed, "Random seed cannot be negative.");
        }

        MaxRounds = maxRounds;
        ShuffleDeck = shuffleDeck;
        RandomSeed = randomSeed;
    }

    /// <summary>
    /// Gets the maximum number of rounds to play.
    /// </summary>
    public int MaxRounds { get; }

    /// <summary>
    /// Gets a value indicating whether the deck should be shuffled before the match starts.
    /// </summary>
    public bool ShuffleDeck { get; }

    /// <summary>
    /// Gets the deterministic random seed used when shuffling, if provided.
    /// </summary>
    public int? RandomSeed { get; }
}

/// <summary>
/// Describes the state of an active round from the perspective of the server.
/// </summary>
public sealed class GameRoundState
{
    private readonly IReadOnlyDictionary<PlayerId, int> _playerCardIndexes;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameRoundState"/> class.
    /// </summary>
    /// <param name="roundNumber">The one-based round number.</param>
    /// <param name="sharedCardIndex">The index of the shared card visible to all players.</param>
    /// <param name="playerCardIndexes">The mapping of player identifiers to their respective card indexes.</param>
    /// <param name="startedAtUtc">The timestamp in UTC when the round started.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when numeric arguments are invalid.</exception>
    /// <exception cref="ArgumentException">Thrown when player identifiers or card assignments are invalid.</exception>
    public GameRoundState(
        int roundNumber,
        int sharedCardIndex,
        IReadOnlyDictionary<PlayerId, int> playerCardIndexes,
        DateTimeOffset startedAtUtc)
    {
        if (roundNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(roundNumber), roundNumber, "Round number must be greater than zero.");
        }

        if (sharedCardIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sharedCardIndex), sharedCardIndex, "Shared card index cannot be negative.");
        }

        if (playerCardIndexes is null)
        {
            throw new ArgumentNullException(nameof(playerCardIndexes));
        }

        if (playerCardIndexes.Count == 0)
        {
            throw new ArgumentException("At least one player must be associated with the round.", nameof(playerCardIndexes));
        }

        Dictionary<PlayerId, int> normalizedAssignments = new();
        foreach (KeyValuePair<PlayerId, int> assignment in playerCardIndexes)
        {
            if (assignment.Key.IsEmpty)
            {
                throw new ArgumentException("Player identifiers must be defined.", nameof(playerCardIndexes));
            }

            if (assignment.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerCardIndexes), assignment.Value, "Card index cannot be negative.");
            }

            if (normalizedAssignments.ContainsKey(assignment.Key))
            {
                throw new ArgumentException("Duplicate player identifiers detected in round assignments.", nameof(playerCardIndexes));
            }

            normalizedAssignments[assignment.Key] = assignment.Value;
        }

        RoundNumber = roundNumber;
        SharedCardIndex = sharedCardIndex;
        StartedAtUtc = EnsureUtc(startedAtUtc);
        _playerCardIndexes = new ReadOnlyDictionary<PlayerId, int>(normalizedAssignments);
    }

    /// <summary>
    /// Gets the one-based round number.
    /// </summary>
    public int RoundNumber { get; }

    /// <summary>
    /// Gets the index of the shared card visible to all players.
    /// </summary>
    public int SharedCardIndex { get; }

    /// <summary>
    /// Gets the timestamp in UTC when the round started.
    /// </summary>
    public DateTimeOffset StartedAtUtc { get; }

    /// <summary>
    /// Gets the mapping of players to their current card indexes.
    /// </summary>
    public IReadOnlyDictionary<PlayerId, int> PlayerCardIndexes => _playerCardIndexes;

    private static DateTimeOffset EnsureUtc(DateTimeOffset timestamp)
    {
        return timestamp.Offset == TimeSpan.Zero
            ? timestamp
            : timestamp.ToUniversalTime();
    }
}

/// <summary>
/// Represents the outcome of processing a player's attempt during a round.
/// </summary>
public sealed class RoundResolutionResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoundResolutionResult"/> class.
    /// </summary>
    /// <param name="roundResolved">Indicates whether the round finished because of the attempt.</param>
    /// <param name="attemptAccepted">Indicates whether the attempt was considered valid by the game mode.</param>
    /// <param name="resolvingPlayerId">The identifier of the player that resolved the round, when available.</param>
    /// <param name="resolvingPlayerCardIndex">The card index held by the resolving player, when available.</param>
    /// <param name="matchingSymbolId">The symbol identifier that resolved the round, when available.</param>
    /// <param name="roundNumber">The one-based round number affected by the attempt, when available.</param>
    /// <param name="processedAtUtc">The timestamp when the attempt was processed in UTC.</param>
    /// <param name="resolutionDuration">The time elapsed since the start of the round when it resolved.</param>
    /// <exception cref="ArgumentException">Thrown when identifiers are invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when numeric arguments are invalid.</exception>
    public RoundResolutionResult(
        bool roundResolved,
        bool attemptAccepted,
        PlayerId? resolvingPlayerId,
        int? resolvingPlayerCardIndex,
        int? matchingSymbolId,
        int? roundNumber,
        DateTimeOffset processedAtUtc,
        TimeSpan? resolutionDuration)
    {
        if (resolvingPlayerId.HasValue && resolvingPlayerId.Value.IsEmpty)
        {
            throw new ArgumentException("Resolving player identifier must be defined when provided.", nameof(resolvingPlayerId));
        }

        if (resolvingPlayerCardIndex.HasValue && resolvingPlayerCardIndex.Value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(resolvingPlayerCardIndex), resolvingPlayerCardIndex.Value, "Resolving player card index cannot be negative.");
        }

        if (matchingSymbolId.HasValue && matchingSymbolId.Value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(matchingSymbolId), matchingSymbolId.Value, "Matching symbol identifier cannot be negative.");
        }

        if (roundNumber.HasValue && roundNumber.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(roundNumber), roundNumber.Value, "Round number must be greater than zero when provided.");
        }

        if (resolutionDuration.HasValue && resolutionDuration.Value < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(resolutionDuration), resolutionDuration.Value, "Resolution duration cannot be negative.");
        }

        RoundResolved = roundResolved;
        AttemptAccepted = attemptAccepted;
        ResolvingPlayerId = resolvingPlayerId;
        ResolvingPlayerCardIndex = resolvingPlayerCardIndex;
        MatchingSymbolId = matchingSymbolId;
        RoundNumber = roundNumber;
        ProcessedAtUtc = EnsureUtc(processedAtUtc);
        ResolutionDuration = resolutionDuration;
    }

    /// <summary>
    /// Gets a value indicating whether the round finished because of the processed attempt.
    /// </summary>
    public bool RoundResolved { get; }

    /// <summary>
    /// Gets a value indicating whether the attempt was accepted by the game mode.
    /// </summary>
    public bool AttemptAccepted { get; }

    /// <summary>
    /// Gets the identifier of the player that resolved the round, when available.
    /// </summary>
    public PlayerId? ResolvingPlayerId { get; }

    /// <summary>
    /// Gets the card index held by the resolving player, when available.
    /// </summary>
    public int? ResolvingPlayerCardIndex { get; }

    /// <summary>
    /// Gets the identifier of the matching symbol used to resolve the round, when available.
    /// </summary>
    public int? MatchingSymbolId { get; }

    /// <summary>
    /// Gets the one-based round number associated with the attempt, when available.
    /// </summary>
    public int? RoundNumber { get; }

    /// <summary>
    /// Gets the timestamp when the attempt was processed in UTC.
    /// </summary>
    public DateTimeOffset ProcessedAtUtc { get; }

    /// <summary>
    /// Gets the duration between the start of the round and its resolution, when available.
    /// </summary>
    public TimeSpan? ResolutionDuration { get; }

    private static DateTimeOffset EnsureUtc(DateTimeOffset timestamp)
    {
        return timestamp.Offset == TimeSpan.Zero
            ? timestamp
            : timestamp.ToUniversalTime();
    }
}

/// <summary>
/// Represents an immutable snapshot of the scores for all participants in a match.
/// </summary>
public sealed class ScoreSnapshot
{
    private readonly IReadOnlyDictionary<PlayerId, int> _scores;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScoreSnapshot"/> class.
    /// </summary>
    /// <param name="scores">The mapping of player identifiers to their score values.</param>
    /// <param name="capturedAtUtc">The timestamp when the snapshot was captured in UTC.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provided scores dictionary is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the provided dictionary contains invalid identifiers.</exception>
    public ScoreSnapshot(IReadOnlyDictionary<PlayerId, int> scores, DateTimeOffset capturedAtUtc)
    {
        if (scores is null)
        {
            throw new ArgumentNullException(nameof(scores));
        }

        Dictionary<PlayerId, int> normalizedScores = new();
        foreach (KeyValuePair<PlayerId, int> score in scores)
        {
            if (score.Key.IsEmpty)
            {
                throw new ArgumentException("Score snapshot cannot contain empty player identifiers.", nameof(scores));
            }

            normalizedScores[score.Key] = score.Value;
        }

        _scores = new ReadOnlyDictionary<PlayerId, int>(normalizedScores);
        CapturedAtUtc = EnsureUtc(capturedAtUtc);
    }

    /// <summary>
    /// Gets the timestamp when the snapshot was captured in UTC.
    /// </summary>
    public DateTimeOffset CapturedAtUtc { get; }

    /// <summary>
    /// Gets the mapping of player identifiers to their score values.
    /// </summary>
    public IReadOnlyDictionary<PlayerId, int> Scores => _scores;

    private static DateTimeOffset EnsureUtc(DateTimeOffset timestamp)
    {
        return timestamp.Offset == TimeSpan.Zero
            ? timestamp
            : timestamp.ToUniversalTime();
    }
}
