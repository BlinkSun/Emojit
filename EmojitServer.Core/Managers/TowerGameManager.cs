using System;
using System.Collections.Generic;
using System.Linq;
using EmojitServer.Core.Design;
using EmojitServer.Core.GameModes;
using EmojitServer.Domain.Entities;
using EmojitServer.Domain.Enums;
using EmojitServer.Domain.ValueObjects;

namespace EmojitServer.Core.Managers;

/// <summary>
/// Provides orchestration logic for the Tower gameplay mode, coordinating deck flow and scoring.
/// </summary>
public sealed class TowerGameManager : IGameMode
{
    private readonly object _syncRoot = new();

    private GameSession? _session;
    private EmojitDesign? _design;
    private GameModeConfiguration? _configuration;
    private IReadOnlyList<PlayerId>? _participants;
    private IReadOnlyList<int>? _deckOrder;
    private Dictionary<PlayerId, int>? _scores;

    private int _cardsPerRound;
    private int _deckCursor;
    private int _roundNumber;
    private int _maximumPlayableRounds;
    private bool _gameOver;
    private bool _roundResolved;
    private DateTimeOffset? _roundStartedAtUtc;
    private GameRoundState? _currentRound;

    /// <inheritdoc />
    public GameMode Mode => GameMode.Tower;

    /// <inheritdoc />
    public bool IsGameOver
    {
        get
        {
            lock (_syncRoot)
            {
                return _gameOver;
            }
        }
    }

    /// <inheritdoc />
    public GameRoundState? CurrentRound
    {
        get
        {
            lock (_syncRoot)
            {
                return _currentRound;
            }
        }
    }

    /// <inheritdoc />
    public void Initialize(
        GameSession session,
        IReadOnlyCollection<PlayerId> participants,
        EmojitDesign design,
        GameModeConfiguration configuration)
    {
        if (session is null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        if (participants is null)
        {
            throw new ArgumentNullException(nameof(participants));
        }

        if (design is null)
        {
            throw new ArgumentNullException(nameof(design));
        }

        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        lock (_syncRoot)
        {
            if (_session is not null)
            {
                throw new InvalidOperationException("The Tower game manager has already been initialized.");
            }

            if (session.Mode != GameMode.Tower)
            {
                throw new ArgumentException("The provided session is not configured for the Tower mode.", nameof(session));
            }

            IReadOnlyList<PlayerId> orderedParticipants = participants
                .Where(player => !player.IsEmpty)
                .ToArray();

            if (orderedParticipants.Count == 0)
            {
                throw new ArgumentException("At least one participant is required to initialize the Tower mode.", nameof(participants));
            }

            Dictionary<PlayerId, int> scoreboard = new();
            foreach (PlayerId participant in orderedParticipants)
            {
                if (scoreboard.ContainsKey(participant))
                {
                    throw new ArgumentException("Duplicate participants detected while initializing the Tower mode.", nameof(participants));
                }

                scoreboard[participant] = 0;
            }

            List<int> deckOrder = BuildDeckOrder(design.CardCount, configuration.ShuffleDeck, configuration.RandomSeed);

            _session = session;
            _design = design;
            _configuration = configuration;
            _participants = orderedParticipants;
            _deckOrder = deckOrder;
            _scores = scoreboard;

            _cardsPerRound = orderedParticipants.Count + 1;
            _deckCursor = 0;
            _roundNumber = 0;
            _roundResolved = true;
            _currentRound = null;
            _roundStartedAtUtc = null;
            _gameOver = false;

            int potentialRounds = deckOrder.Count / _cardsPerRound;
            if (potentialRounds == 0)
            {
                throw new InvalidOperationException("The configured deck does not contain enough cards to start a Tower round.");
            }

            _maximumPlayableRounds = Math.Min(configuration.MaxRounds, potentialRounds);
            if (_maximumPlayableRounds == 0)
            {
                _gameOver = true;
            }
        }
    }

    /// <inheritdoc />
    public GameRoundState StartNextRound(DateTimeOffset startedAtUtc)
    {
        lock (_syncRoot)
        {
            EnsureInitialized();

            if (_gameOver)
            {
                throw new InvalidOperationException("The Tower match has already concluded.");
            }

            if (!_roundResolved)
            {
                throw new InvalidOperationException("Cannot start a new round before the current round resolves.");
            }

            if (_roundNumber >= _maximumPlayableRounds)
            {
                _gameOver = true;
                throw new InvalidOperationException("The Tower match reached the configured round limit.");
            }

            try
            {
                int sharedCardIndex = DrawCard();

                Dictionary<PlayerId, int> assignments = new();
                IReadOnlyList<PlayerId> participants = _participants!;
                foreach (PlayerId participant in participants)
                {
                    int participantCard = DrawCard();
                    assignments[participant] = participantCard;
                }

                _roundNumber++;
                _roundResolved = false;
                _roundStartedAtUtc = EnsureUtc(startedAtUtc);

                _currentRound = new GameRoundState(
                    _roundNumber,
                    sharedCardIndex,
                    assignments,
                    _roundStartedAtUtc.Value);

                return _currentRound;
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or ArgumentOutOfRangeException)
            {
                _gameOver = true;
                throw;
            }
            catch (Exception ex)
            {
                _gameOver = true;
                throw new InvalidOperationException("Failed to start the next Tower round due to an unexpected error.", ex);
            }
        }
    }

    /// <inheritdoc />
    public RoundResolutionResult RegisterAttempt(PlayerId playerId, int symbolId, DateTimeOffset occurredAtUtc)
    {
        lock (_syncRoot)
        {
            EnsureInitialized();

            if (playerId.IsEmpty)
            {
                throw new ArgumentException("Player identifier must be provided when registering an attempt.", nameof(playerId));
            }

            if (symbolId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(symbolId), symbolId, "Symbol identifier cannot be negative.");
            }

            if (_currentRound is null)
            {
                throw new InvalidOperationException("No active round is available to register attempts against.");
            }

            if (!_scores!.ContainsKey(playerId))
            {
                throw new InvalidOperationException("The specified player is not part of the running match.");
            }

            DateTimeOffset processedAtUtc = EnsureUtc(occurredAtUtc);

            if (_roundResolved)
            {
                return new RoundResolutionResult(
                    roundResolved: true,
                    attemptAccepted: false,
                    resolvingPlayerId: null,
                    resolvingPlayerCardIndex: null,
                    matchingSymbolId: null,
                    roundNumber: _roundNumber,
                    processedAtUtc: processedAtUtc,
                    resolutionDuration: _roundStartedAtUtc.HasValue ? processedAtUtc - _roundStartedAtUtc : null);
            }

            if (!_currentRound.PlayerCardIndexes.TryGetValue(playerId, out int playerCardIndex))
            {
                throw new InvalidOperationException("The player does not have a card assigned in the current round.");
            }

            int sharedCardIndex = _currentRound.SharedCardIndex;

            int matchingSymbol;
            try
            {
                matchingSymbol = _design!.FindCommonSymbol(sharedCardIndex, playerCardIndex);
            }
            catch (Exception ex) when (ex is ArgumentOutOfRangeException or InvalidOperationException)
            {
                _roundResolved = true;
                _gameOver = true;
                throw;
            }
            catch (Exception ex)
            {
                _roundResolved = true;
                _gameOver = true;
                throw new InvalidOperationException("An unexpected error occurred while validating the attempt.", ex);
            }

            bool isCorrect = matchingSymbol == symbolId;
            if (!isCorrect)
            {
                return new RoundResolutionResult(
                    roundResolved: false,
                    attemptAccepted: true,
                    resolvingPlayerId: null,
                    resolvingPlayerCardIndex: playerCardIndex,
                    matchingSymbolId: null,
                    roundNumber: _roundNumber,
                    processedAtUtc: processedAtUtc,
                    resolutionDuration: null);
            }

            _scores[playerId] = _scores[playerId] + 1;
            _roundResolved = true;
            _currentRound = null;

            TimeSpan? resolutionDuration = _roundStartedAtUtc.HasValue
                ? processedAtUtc - _roundStartedAtUtc
                : null;

            EvaluateCompletionState();

            return new RoundResolutionResult(
                roundResolved: true,
                attemptAccepted: true,
                resolvingPlayerId: playerId,
                resolvingPlayerCardIndex: playerCardIndex,
                matchingSymbolId: matchingSymbol,
                roundNumber: _roundNumber,
                processedAtUtc: processedAtUtc,
                resolutionDuration: resolutionDuration);
        }
    }

    /// <inheritdoc />
    public ScoreSnapshot GetScoreSnapshot()
    {
        lock (_syncRoot)
        {
            EnsureInitialized();
            return new ScoreSnapshot(new Dictionary<PlayerId, int>(_scores!), DateTimeOffset.UtcNow);
        }
    }

    private void EvaluateCompletionState()
    {
        if (_roundNumber >= _maximumPlayableRounds)
        {
            _gameOver = true;
            return;
        }

        if (_deckOrder is null)
        {
            _gameOver = true;
            return;
        }

        if (_deckCursor + _cardsPerRound > _deckOrder.Count)
        {
            _gameOver = true;
        }
    }

    private int DrawCard()
    {
        if (_deckOrder is null)
        {
            throw new InvalidOperationException("The deck order has not been initialized.");
        }

        if (_deckCursor >= _deckOrder.Count)
        {
            throw new InvalidOperationException("The deck does not contain enough cards for another draw.");
        }

        int cardIndex = _deckOrder[_deckCursor];
        _deckCursor++;
        return cardIndex;
    }

    private void EnsureInitialized()
    {
        if (_session is null || _design is null || _configuration is null || _participants is null || _deckOrder is null || _scores is null)
        {
            throw new InvalidOperationException("The Tower game manager must be initialized before use.");
        }
    }

    private static List<int> BuildDeckOrder(int cardCount, bool shuffle, int? seed)
    {
        if (cardCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cardCount), cardCount, "The deck must contain at least one card.");
        }

        List<int> cards = new(cardCount);
        for (int index = 0; index < cardCount; index++)
        {
            cards.Add(index);
        }

        if (!shuffle || cardCount == 1)
        {
            return cards;
        }

        Random random = seed.HasValue ? new Random(seed.Value) : new Random();
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int swapIndex = random.Next(i + 1);
            (cards[i], cards[swapIndex]) = (cards[swapIndex], cards[i]);
        }

        return cards;
    }

    private static DateTimeOffset EnsureUtc(DateTimeOffset timestamp)
    {
        return timestamp.Offset == TimeSpan.Zero
            ? timestamp
            : timestamp.ToUniversalTime();
    }
}
