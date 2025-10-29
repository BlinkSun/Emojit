using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EmojitServer.Application.Abstractions.Repositories;
using EmojitServer.Application.Abstractions.Services;
using EmojitServer.Application.Configuration;
using EmojitServer.Application.Services;
using EmojitServer.Application.Services.Models;
using EmojitServer.Core.Design;
using EmojitServer.Core.GameModes;
using EmojitServer.Domain.Entities;
using EmojitServer.Domain.Enums;
using EmojitServer.Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EmojitServer.Tests.Application;

/// <summary>
/// Validates the primary happy path interactions exposed by <see cref="GameService"/>.
/// </summary>
public sealed class GameServiceFlowTests
{
    /// <summary>
    /// Verifies that the <see cref="GameService"/> can orchestrate an entire session lifecycle using in-memory collaborators.
    /// </summary>
    [Fact]
    public async Task Should_Create_Start_And_Complete_Game_Flow()
    {
        InMemoryGameSessionRepository sessionRepository = new();
        InMemoryPlayerRepository playerRepository = new();
        TrackingLogService logService = new();
        TrackingLeaderboardService leaderboardService = new();
        TestValidationService validationService = new(playerRepository);

        GameDefaultsOptions defaults = new()
        {
            DefaultMode = GameMode.Tower,
            DefaultMaxPlayers = 4,
            MinimumPlayers = 2,
            MaximumPlayers = 6,
            DefaultMaxRounds = 1,
            MinimumRounds = 1,
            MaximumRounds = 5,
            DesignOrder = 3,
            ShuffleDeck = false
        };

        GameService service = new(
            sessionRepository,
            playerRepository,
            validationService,
            logService,
            leaderboardService,
            Options.Create(defaults),
            NullLogger<GameService>.Instance);

        CancellationToken cancellationToken = CancellationToken.None;

        GameSession session = await service.CreateGameAsync(GameMode.Tower, 2, 1, cancellationToken).ConfigureAwait(false);
        Assert.Equal(GameMode.Tower, session.Mode);
        Assert.Equal(2, session.MaxPlayers);
        Assert.Equal(1, session.MaxRounds);

        Player playerOne = Player.Create(PlayerId.New(), "Alice", DateTimeOffset.UtcNow);
        Player playerTwo = Player.Create(PlayerId.New(), "Bob", DateTimeOffset.UtcNow);
        await playerRepository.AddAsync(playerOne, cancellationToken).ConfigureAwait(false);
        await playerRepository.AddAsync(playerTwo, cancellationToken).ConfigureAwait(false);

        await service.JoinGameAsync(session.Id, playerOne.Id, cancellationToken).ConfigureAwait(false);
        await service.JoinGameAsync(session.Id, playerTwo.Id, cancellationToken).ConfigureAwait(false);

        GameRoundState firstRound = await service.StartGameAsync(session.Id, cancellationToken).ConfigureAwait(false);
        Assert.Equal(1, firstRound.RoundNumber);
        Assert.Equal(2, firstRound.PlayerCardIndexes.Count);

        EmojitDesign design = EmojitDesign.Create(defaults.DesignOrder);
        int playerOneCard = firstRound.PlayerCardIndexes[playerOne.Id];
        int matchingSymbol = design.FindCommonSymbol(firstRound.SharedCardIndex, playerOneCard);

        GameAttemptResult attemptResult = await service.ClickSymbolAsync(session.Id, playerOne.Id, matchingSymbol, cancellationToken).ConfigureAwait(false);

        Assert.True(attemptResult.Resolution.RoundResolved);
        Assert.True(attemptResult.Resolution.AttemptAccepted);
        Assert.True(attemptResult.GameCompleted);
        Assert.Null(attemptResult.NextRound);
        Assert.NotNull(attemptResult.ScoreSnapshot);
        Assert.Equal(1, attemptResult.ScoreSnapshot!.Scores[playerOne.Id]);

        GameSession persisted = await service.PersistEndGameAsync(session.Id, cancellationToken).ConfigureAwait(false);
        Assert.True(persisted.IsCompleted);
        Assert.Equal(session.Id, persisted.Id);

        Assert.Single(logService.GetLogs(session.Id));
        Assert.True(leaderboardService.WasUpdated);

        Player updatedPlayerOne = await playerRepository.GetByIdAsync(playerOne.Id, cancellationToken).ConfigureAwait(false) ?? throw new InvalidOperationException("Player not found after update.");
        Assert.Equal(1, updatedPlayerOne.GamesPlayed);
        Assert.Equal(1, updatedPlayerOne.GamesWon);
    }

    /// <summary>
    /// Provides an in-memory implementation of <see cref="IGameSessionRepository"/> for testing scenarios.
    /// </summary>
    private sealed class InMemoryGameSessionRepository : IGameSessionRepository
    {
        private readonly ConcurrentDictionary<GameId, GameSession> _sessions = new();

        public Task AddAsync(GameSession gameSession, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(gameSession);
            _sessions[gameSession.Id] = gameSession;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<GameSession>> GetActiveSessionsAsync(CancellationToken cancellationToken)
        {
            IReadOnlyCollection<GameSession> activeSessions = _sessions.Values
                .Where(session => session.IsStarted && !session.IsCompleted)
                .ToArray();
            return Task.FromResult(activeSessions);
        }

        public Task<GameSession?> GetByIdAsync(GameId gameId, CancellationToken cancellationToken)
        {
            _sessions.TryGetValue(gameId, out GameSession? session);
            return Task.FromResult(session);
        }

        public Task UpdateAsync(GameSession gameSession, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(gameSession);
            _sessions[gameSession.Id] = gameSession;
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Supplies an in-memory player repository used by the validation service.
    /// </summary>
    private sealed class InMemoryPlayerRepository : IPlayerRepository
    {
        private readonly ConcurrentDictionary<PlayerId, Player> _players = new();

        public Task AddAsync(Player player, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(player);
            _players[player.Id] = player;
            return Task.CompletedTask;
        }

        public Task<Player?> GetByIdAsync(PlayerId playerId, CancellationToken cancellationToken)
        {
            _players.TryGetValue(playerId, out Player? player);
            return Task.FromResult(player);
        }

        public Task UpdateAsync(Player player, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(player);
            _players[player.Id] = player;
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Mimics the production validation service with simplified in-memory checks.
    /// </summary>
    private sealed class TestValidationService : IValidationService
    {
        private readonly InMemoryPlayerRepository _playerRepository;

        public TestValidationService(InMemoryPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public void EnsureAttemptAllowed(GameSession session, PlayerId playerId)
        {
            EnsureSessionIsActive(session);

            if (!session.Participants.Contains(playerId))
            {
                throw new InvalidOperationException("Player is not part of the session.");
            }
        }

        public async Task<Player> EnsurePlayerExistsAsync(PlayerId playerId, CancellationToken cancellationToken)
        {
            Player? player = await _playerRepository.GetByIdAsync(playerId, cancellationToken).ConfigureAwait(false);
            if (player is null)
            {
                throw new InvalidOperationException("Player not found.");
            }

            return player;
        }

        public void EnsurePlayerCanJoin(GameSession session, PlayerId playerId)
        {
            if (session.IsStarted)
            {
                throw new InvalidOperationException("Session already started.");
            }

            if (session.IsCompleted)
            {
                throw new InvalidOperationException("Session already completed.");
            }

            if (session.Participants.Contains(playerId))
            {
                throw new InvalidOperationException("Player already joined the session.");
            }

            if (session.Participants.Count >= session.MaxPlayers)
            {
                throw new InvalidOperationException("Session is full.");
            }
        }

        public void EnsureSessionCanStart(GameSession session)
        {
            if (session.IsStarted)
            {
                throw new InvalidOperationException("Session already started.");
            }

            if (session.IsCompleted)
            {
                throw new InvalidOperationException("Session already completed.");
            }

            if (session.Participants.Count < 2)
            {
                throw new InvalidOperationException("Not enough participants.");
            }
        }

        public void EnsureSessionIsActive(GameSession session)
        {
            if (!session.IsStarted)
            {
                throw new InvalidOperationException("Session has not started.");
            }

            if (session.IsCompleted)
            {
                throw new InvalidOperationException("Session already completed.");
            }
        }
    }

    /// <summary>
    /// Captures round logs produced during the test flow for later inspection.
    /// </summary>
    private sealed class TrackingLogService : ILogService
    {
        private readonly List<RoundLog> _roundLogs = new();

        public RoundLog CreateRoundLog(GameSession session, GameRoundState roundState, RoundResolutionResult resolution)
        {
            if (!resolution.RoundNumber.HasValue)
            {
                throw new InvalidOperationException("Round number is required to create a log.");
            }

            if (!resolution.MatchingSymbolId.HasValue)
            {
                throw new InvalidOperationException("Matching symbol is required to create a log.");
            }

            PlayerId? winner = resolution.ResolvingPlayerId;
            int? winnerCardIndex = resolution.ResolvingPlayerCardIndex;

            if (winner.HasValue && !winnerCardIndex.HasValue && roundState.PlayerCardIndexes.TryGetValue(winner.Value, out int resolvedCard))
            {
                winnerCardIndex = resolvedCard;
            }

            return RoundLog.Create(
                session.Id,
                resolution.RoundNumber.Value,
                winner,
                roundState.SharedCardIndex,
                winnerCardIndex,
                resolution.MatchingSymbolId.Value,
                resolution.ProcessedAtUtc,
                resolution.ResolutionDuration ?? TimeSpan.Zero);
        }

        public IReadOnlyList<RoundLog> GetLogs(GameId gameId)
        {
            return _roundLogs.Where(log => log.GameId == gameId).ToArray();
        }

        public Task<IReadOnlyList<RoundLog>> GetLogsAsync(GameId gameId, CancellationToken cancellationToken)
        {
            IReadOnlyList<RoundLog> logs = GetLogs(gameId);
            return Task.FromResult(logs);
        }

        public Task PersistRoundLogAsync(RoundLog roundLog, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(roundLog);
            _roundLogs.Add(roundLog);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Records whether the leaderboard was updated while allowing retrieval assertions.
    /// </summary>
    private sealed class TrackingLeaderboardService : ILeaderboardService
    {
        public bool WasUpdated { get; private set; }

        public Task<IReadOnlyList<LeaderboardEntry>> GetTopAsync(int count, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<LeaderboardEntry>>(Array.Empty<LeaderboardEntry>());
        }

        public Task UpdateLeaderboardAsync(GameSession session, ScoreSnapshot scores, CancellationToken cancellationToken)
        {
            WasUpdated = true;
            return Task.CompletedTask;
        }
    }
}
