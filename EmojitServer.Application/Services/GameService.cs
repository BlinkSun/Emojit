using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EmojitServer.Application.Abstractions.Repositories;
using EmojitServer.Application.Abstractions.Services;
using EmojitServer.Application.Services.Models;
using EmojitServer.Core.Design;
using EmojitServer.Core.GameModes;
using EmojitServer.Core.Managers;
using EmojitServer.Domain.Entities;
using EmojitServer.Domain.Enums;
using EmojitServer.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace EmojitServer.Application.Services;

/// <summary>
/// Provides orchestration logic for scheduling and executing Emojit games.
/// </summary>
public sealed class GameService : IGameService
{
    private const int DefaultDesignOrder = 7;

    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IValidationService _validationService;
    private readonly ILogService _logService;
    private readonly ILeaderboardService _leaderboardService;
    private readonly ILogger<GameService> _logger;

    private readonly ConcurrentDictionary<GameId, ActiveGameRuntime> _activeGames = new();
    private readonly ConcurrentDictionary<GameId, SemaphoreSlim> _sessionLocks = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GameService"/> class.
    /// </summary>
    /// <param name="gameSessionRepository">The repository used to persist game sessions.</param>
    /// <param name="playerRepository">The repository used to access player data.</param>
    /// <param name="validationService">The validation service.</param>
    /// <param name="logService">The logging service.</param>
    /// <param name="leaderboardService">The leaderboard service.</param>
    /// <param name="logger">The logger instance.</param>
    public GameService(
        IGameSessionRepository gameSessionRepository,
        IPlayerRepository playerRepository,
        IValidationService validationService,
        ILogService logService,
        ILeaderboardService leaderboardService,
        ILogger<GameService> logger)
    {
        _gameSessionRepository = gameSessionRepository;
        _playerRepository = playerRepository;
        _validationService = validationService;
        _logService = logService;
        _leaderboardService = leaderboardService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<GameSession> CreateGameAsync(GameMode mode, int maxPlayers, int maxRounds, CancellationToken cancellationToken)
    {
        try
        {
            GameSession session = GameSession.Schedule(GameId.New(), mode, maxPlayers, maxRounds, DateTimeOffset.UtcNow);
            await _gameSessionRepository.AddAsync(session, cancellationToken).ConfigureAwait(false);
            _sessionLocks.TryAdd(session.Id, new SemaphoreSlim(1, 1));
            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create game session for mode {Mode}.", mode);
            throw new InvalidOperationException("Failed to create game session due to an unexpected error.", ex);
        }
    }

    /// <inheritdoc />
    public async Task JoinGameAsync(GameId gameId, PlayerId playerId, CancellationToken cancellationToken)
    {
        SemaphoreSlim sessionLock = GetSessionLock(gameId);
        await sessionLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            GameSession? session = await _gameSessionRepository.GetByIdAsync(gameId, cancellationToken).ConfigureAwait(false);
            if (session is null)
            {
                throw new InvalidOperationException($"Game session '{gameId}' was not found.");
            }

            Player player = await _validationService.EnsurePlayerExistsAsync(playerId, cancellationToken).ConfigureAwait(false);
            _validationService.EnsurePlayerCanJoin(session, playerId);

            session.AddParticipant(playerId);
            player.Touch(DateTimeOffset.UtcNow);

            await _playerRepository.UpdateAsync(player, cancellationToken).ConfigureAwait(false);
            await _gameSessionRepository.UpdateAsync(session, cancellationToken).ConfigureAwait(false);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add player {PlayerId} to game {GameId}.", playerId, gameId);
            throw new InvalidOperationException("Failed to join the game due to an unexpected error.", ex);
        }
        finally
        {
            sessionLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<GameRoundState> StartGameAsync(GameId gameId, CancellationToken cancellationToken)
    {
        SemaphoreSlim sessionLock = GetSessionLock(gameId);
        await sessionLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (_activeGames.ContainsKey(gameId))
            {
                throw new InvalidOperationException("The game session is already running.");
            }

            GameSession? session = await _gameSessionRepository.GetByIdAsync(gameId, cancellationToken).ConfigureAwait(false);
            if (session is null)
            {
                throw new InvalidOperationException($"Game session '{gameId}' was not found.");
            }

            _validationService.EnsureSessionCanStart(session);

            IGameMode gameMode = CreateGameMode(session.Mode);
            EmojitDesign design = CreateDesign();
            GameModeConfiguration configuration = new(session.MaxRounds);

            try
            {
                gameMode.Initialize(session, session.Participants, design, configuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize game mode for game {GameId}.", gameId);
                throw new InvalidOperationException("Failed to initialize the game mode.", ex);
            }

            DateTimeOffset now = DateTimeOffset.UtcNow;
            session.Start(now);

            GameRoundState firstRound;
            try
            {
                firstRound = gameMode.StartNextRound(now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start the first round for game {GameId}.", gameId);
                throw new InvalidOperationException("Failed to start the first round due to an unexpected error.", ex);
            }

            ActiveGameRuntime runtime = new(session, gameMode, design);
            if (!_activeGames.TryAdd(gameId, runtime))
            {
                throw new InvalidOperationException("The game session is already registered as active.");
            }

            await _gameSessionRepository.UpdateAsync(session, cancellationToken).ConfigureAwait(false);
            return firstRound;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start game {GameId}.", gameId);
            throw new InvalidOperationException("Failed to start the game due to an unexpected error.", ex);
        }
        finally
        {
            sessionLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<GameAttemptResult> ClickSymbolAsync(GameId gameId, PlayerId playerId, int symbolId, CancellationToken cancellationToken)
    {
        SemaphoreSlim sessionLock = GetSessionLock(gameId);
        await sessionLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (!_activeGames.TryGetValue(gameId, out ActiveGameRuntime? runtime))
            {
                throw new InvalidOperationException("The game session is not active.");
            }

            _validationService.EnsureAttemptAllowed(runtime.Session, playerId);

            GameRoundState? roundState = runtime.GameMode.CurrentRound;
            if (roundState is null)
            {
                throw new InvalidOperationException("No active round is currently available.");
            }

            RoundResolutionResult resolution = runtime.GameMode.RegisterAttempt(playerId, symbolId, DateTimeOffset.UtcNow);

            GameRoundState? nextRound = null;
            ScoreSnapshot? snapshot = null;
            bool gameCompleted = false;

            if (resolution.AttemptAccepted)
            {
                snapshot = runtime.GameMode.GetScoreSnapshot();
            }

            if (resolution.RoundResolved && resolution.AttemptAccepted)
            {
                RoundLog roundLog = _logService.CreateRoundLog(runtime.Session, roundState, resolution);
                runtime.Session.RegisterRound(roundLog);

                try
                {
                    await _logService.PersistRoundLogAsync(roundLog, cancellationToken).ConfigureAwait(false);
                    await _gameSessionRepository.UpdateAsync(runtime.Session, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to persist round result for game {GameId}.", gameId);
                    throw new InvalidOperationException("Failed to record the round result due to an unexpected error.", ex);
                }

                if (runtime.GameMode.IsGameOver)
                {
                    await PersistEndGameInternalAsync(gameId, runtime, cancellationToken).ConfigureAwait(false);
                    gameCompleted = true;
                }
                else
                {
                    try
                    {
                        nextRound = runtime.GameMode.StartNextRound(DateTimeOffset.UtcNow);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to start next round for game {GameId}.", gameId);
                        await PersistEndGameInternalAsync(gameId, runtime, cancellationToken).ConfigureAwait(false);
                        gameCompleted = true;
                        nextRound = null;
                    }
                }
            }

            return new GameAttemptResult(resolution, nextRound, gameCompleted, snapshot);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process attempt for player {PlayerId} in game {GameId}.", playerId, gameId);
            throw new InvalidOperationException("Failed to process the attempt due to an unexpected error.", ex);
        }
        finally
        {
            sessionLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<ScoreSnapshot> GetScoresSnapshotAsync(GameId gameId, CancellationToken cancellationToken)
    {
        SemaphoreSlim sessionLock = GetSessionLock(gameId);
        await sessionLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (!_activeGames.TryGetValue(gameId, out ActiveGameRuntime? runtime))
            {
                throw new InvalidOperationException("The game session is not active.");
            }

            return runtime.GameMode.GetScoreSnapshot();
        }
        finally
        {
            sessionLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<GameSession> PersistEndGameAsync(GameId gameId, CancellationToken cancellationToken)
    {
        SemaphoreSlim sessionLock = GetSessionLock(gameId);
        await sessionLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (_activeGames.TryGetValue(gameId, out ActiveGameRuntime? runtime))
            {
                await PersistEndGameInternalAsync(gameId, runtime, cancellationToken).ConfigureAwait(false);
                return runtime.Session;
            }

            GameSession? session = await _gameSessionRepository.GetByIdAsync(gameId, cancellationToken).ConfigureAwait(false);
            if (session is null)
            {
                throw new InvalidOperationException($"Game session '{gameId}' was not found.");
            }

            if (!session.IsCompleted)
            {
                throw new InvalidOperationException("The session is not completed and no runtime state is available.");
            }

            return session;
        }
        finally
        {
            sessionLock.Release();
        }
    }

    private SemaphoreSlim GetSessionLock(GameId gameId)
    {
        return _sessionLocks.GetOrAdd(gameId, _ => new SemaphoreSlim(1, 1));
    }

    private static IGameMode CreateGameMode(GameMode mode)
    {
        return mode switch
        {
            GameMode.Tower => new TowerGameManager(),
            _ => throw new NotSupportedException($"Game mode '{mode}' is not supported yet."),
        };
    }

    private static EmojitDesign CreateDesign()
    {
        return EmojitDesign.Create(DefaultDesignOrder);
    }

    private async Task PersistEndGameInternalAsync(GameId gameId, ActiveGameRuntime runtime, CancellationToken cancellationToken)
    {
        if (!_activeGames.ContainsKey(gameId))
        {
            return;
        }

        ScoreSnapshot finalScores = runtime.GameMode.GetScoreSnapshot();
        DateTimeOffset completionTimestamp = DateTimeOffset.UtcNow;

        try
        {
            if (!runtime.Session.IsCompleted)
            {
                runtime.Session.Complete(completionTimestamp);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark game {GameId} as completed.", gameId);
            throw new InvalidOperationException("Failed to mark the session as completed.", ex);
        }

        HashSet<PlayerId> winners = DetermineWinners(finalScores);

        foreach (PlayerId participant in runtime.Session.Participants)
        {
            Player player = await _validationService.EnsurePlayerExistsAsync(participant, cancellationToken).ConfigureAwait(false);
            bool won = winners.Contains(participant);

            try
            {
                player.RegisterGameResult(won, completionTimestamp);
                await _playerRepository.UpdateAsync(player, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update player {PlayerId} after game {GameId} completion.", participant, gameId);
                throw new InvalidOperationException("Failed to persist player updates after completing the game.", ex);
            }
        }

        try
        {
            await _leaderboardService.UpdateLeaderboardAsync(runtime.Session, finalScores, cancellationToken).ConfigureAwait(false);
            await _gameSessionRepository.UpdateAsync(runtime.Session, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to finalize persistence for game {GameId}.", gameId);
            throw new InvalidOperationException("Failed to finalize the game due to an unexpected error.", ex);
        }

        _activeGames.TryRemove(gameId, out _);
    }

    private static HashSet<PlayerId> DetermineWinners(ScoreSnapshot snapshot)
    {
        if (snapshot.Scores.Count == 0)
        {
            return new HashSet<PlayerId>();
        }

        int bestScore = snapshot.Scores.Values.Max();
        return snapshot.Scores
            .Where(pair => pair.Value == bestScore)
            .Select(pair => pair.Key)
            .ToHashSet();
    }

    private sealed class ActiveGameRuntime
    {
        public ActiveGameRuntime(GameSession session, IGameMode gameMode, EmojitDesign design)
        {
            Session = session;
            GameMode = gameMode;
            Design = design;
        }

        public GameSession Session { get; }

        public IGameMode GameMode { get; }

        public EmojitDesign Design { get; }
    }
}
