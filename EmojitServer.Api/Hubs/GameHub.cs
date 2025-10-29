using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EmojitServer.Api.Models.Realtime;
using EmojitServer.Application.Abstractions.Services;
using EmojitServer.Application.Services.Models;
using EmojitServer.Core.GameModes;
using EmojitServer.Domain.Entities;
using EmojitServer.Domain.Enums;
using EmojitServer.Domain.ValueObjects;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace EmojitServer.Api.Hubs;

/// <summary>
/// Provides real-time orchestration and notifications for Emojit game sessions.
/// </summary>
public sealed class GameHub : Hub
{
    private const string GroupPrefix = "game:";

    private readonly IGameService _gameService;
    private readonly ILogger<GameHub> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameHub"/> class.
    /// </summary>
    /// <param name="gameService">The application service coordinating gameplay.</param>
    /// <param name="logger">The logger instance.</param>
    public GameHub(IGameService gameService, ILogger<GameHub> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new game session with the provided parameters.
    /// </summary>
    /// <param name="request">The creation payload.</param>
    /// <returns>The descriptor of the scheduled session.</returns>
    public async Task<GameCreatedResponse> CreateGame(CreateGameRequest request)
    {
        if (request is null)
        {
            throw new HubException("A creation payload must be provided.");
        }

        CancellationToken cancellationToken = Context.ConnectionAborted;

        try
        {
            GameSession session = await _gameService
                .CreateGameAsync(request.Mode, request.MaxPlayers, request.MaxRounds, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation(
                "Game {GameId} created by connection {ConnectionId} in mode {Mode}.",
                session.Id,
                Context.ConnectionId,
                session.Mode);

            return new GameCreatedResponse(session.Id.Value.ToString(), session.Mode, session.MaxPlayers, session.MaxRounds);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid payload provided while creating a game from connection {ConnectionId}.", Context.ConnectionId);
            throw new HubException(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business validation failed while creating a game from connection {ConnectionId}.", Context.ConnectionId);
            throw new HubException(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating a game from connection {ConnectionId}.", Context.ConnectionId);
            throw new HubException("An unexpected error occurred while creating the game.");
        }
    }

    /// <summary>
    /// Adds the current connection to an existing game session.
    /// </summary>
    /// <param name="request">The join payload containing identifiers.</param>
    public async Task JoinGame(JoinGameRequest request)
    {
        if (request is null)
        {
            throw new HubException("A join payload must be provided.");
        }

        GameId gameId = ParseGameId(request.GameId);
        PlayerId playerId = ParsePlayerId(request.PlayerId);
        string groupName = GetGroupName(gameId);
        CancellationToken cancellationToken = Context.ConnectionAborted;

        try
        {
            await _gameService.JoinGameAsync(gameId, playerId, cancellationToken).ConfigureAwait(false);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName).ConfigureAwait(false);

            _logger.LogInformation(
                "Player {PlayerId} joined game {GameId} via connection {ConnectionId}.",
                playerId,
                gameId,
                Context.ConnectionId);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid identifiers provided while joining game {GameId} from connection {ConnectionId}.", request.GameId, Context.ConnectionId);
            throw new HubException(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business validation failed while player {PlayerId} attempted to join game {GameId}.", playerId, gameId);
            throw new HubException(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while player {PlayerId} attempted to join game {GameId}.", playerId, gameId);
            throw new HubException("An unexpected error occurred while joining the game.");
        }
    }

    /// <summary>
    /// Starts a scheduled game session and announces the first round to all participants.
    /// </summary>
    /// <param name="gameId">The identifier of the session to start.</param>
    /// <returns>The descriptor of the first round.</returns>
    public async Task<RoundStartEvent> StartGame(string gameId)
    {
        GameId parsedGameId = ParseGameId(gameId);
        string groupName = GetGroupName(parsedGameId);
        CancellationToken cancellationToken = Context.ConnectionAborted;

        try
        {
            GameRoundState roundState = await _gameService
                .StartGameAsync(parsedGameId, cancellationToken)
                .ConfigureAwait(false);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName).ConfigureAwait(false);

            RoundStartEvent startEvent = CreateRoundStartEvent(parsedGameId, roundState);

            await Clients.Group(groupName).SendAsync("RoundStart", startEvent).ConfigureAwait(false);

            _logger.LogInformation("Game {GameId} started by connection {ConnectionId}.", parsedGameId, Context.ConnectionId);

            return startEvent;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid identifier provided while starting game {GameId} from connection {ConnectionId}.", gameId, Context.ConnectionId);
            throw new HubException(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business validation failed while attempting to start game {GameId}.", parsedGameId);
            throw new HubException(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while starting game {GameId} from connection {ConnectionId}.", parsedGameId, Context.ConnectionId);
            throw new HubException("An unexpected error occurred while starting the game.");
        }
    }

    /// <summary>
    /// Processes a player's attempt to resolve the current round.
    /// </summary>
    /// <param name="request">The attempt payload.</param>
    /// <returns>The resolution event reflecting the processed attempt.</returns>
    public async Task<RoundResultEvent> ClickSymbol(ClickSymbolRequest request)
    {
        if (request is null)
        {
            throw new HubException("An attempt payload must be provided.");
        }

        GameId gameId = ParseGameId(request.GameId);
        PlayerId playerId = ParsePlayerId(request.PlayerId);
        string groupName = GetGroupName(gameId);
        CancellationToken cancellationToken = Context.ConnectionAborted;

        try
        {
            GameAttemptResult result = await _gameService
                .ClickSymbolAsync(gameId, playerId, request.SymbolId, cancellationToken)
                .ConfigureAwait(false);

            RoundResultEvent resultEvent = CreateRoundResultEvent(gameId, result);
            await Clients.Group(groupName).SendAsync("RoundResult", resultEvent).ConfigureAwait(false);

            if (result.GameCompleted && result.ScoreSnapshot is not null)
            {
                GameOverEvent gameOverEvent = CreateGameOverEvent(gameId, result.ScoreSnapshot);
                await Clients.Group(groupName).SendAsync("GameOver", gameOverEvent).ConfigureAwait(false);
            }
            else if (result.NextRound is not null)
            {
                RoundStartEvent nextRoundEvent = CreateRoundStartEvent(gameId, result.NextRound);
                await Clients.Group(groupName).SendAsync("RoundStart", nextRoundEvent).ConfigureAwait(false);
            }

            _logger.LogInformation(
                "Processed attempt for player {PlayerId} in game {GameId}. Accepted={Accepted}, Resolved={Resolved}.",
                playerId,
                gameId,
                result.Resolution.AttemptAccepted,
                result.Resolution.RoundResolved);

            return resultEvent;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid identifiers provided while player {PlayerId} clicked symbol in game {GameId}.", request.PlayerId, request.GameId);
            throw new HubException(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business validation failed for player {PlayerId} in game {GameId}.", playerId, gameId);
            throw new HubException(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while processing attempt for player {PlayerId} in game {GameId}.", playerId, gameId);
            throw new HubException("An unexpected error occurred while processing the attempt.");
        }
    }

    private static GameId ParseGameId(string? gameId)
    {
        if (string.IsNullOrWhiteSpace(gameId))
        {
            throw new HubException("A valid game identifier must be supplied.");
        }

        if (!Guid.TryParse(gameId, out Guid guidValue) || !GameId.TryFromGuid(guidValue, out GameId parsed))
        {
            throw new HubException("The provided game identifier is invalid.");
        }

        return parsed;
    }

    private static PlayerId ParsePlayerId(string? playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            throw new HubException("A valid player identifier must be supplied.");
        }

        if (!Guid.TryParse(playerId, out Guid guidValue) || !PlayerId.TryFromGuid(guidValue, out PlayerId parsed))
        {
            throw new HubException("The provided player identifier is invalid.");
        }

        return parsed;
    }

    private static string GetGroupName(GameId gameId)
    {
        return string.Concat(GroupPrefix, gameId.Value.ToString("N"));
    }

    private static RoundStartEvent CreateRoundStartEvent(GameId gameId, GameRoundState roundState)
    {
        IReadOnlyDictionary<string, int> playerCards = roundState.PlayerCardIndexes
            .ToDictionary(pair => pair.Key.Value.ToString(), pair => pair.Value);

        return new RoundStartEvent(
            gameId.Value.ToString(),
            roundState.RoundNumber,
            roundState.SharedCardIndex,
            playerCards,
            roundState.StartedAtUtc);
    }

    private static RoundResultEvent CreateRoundResultEvent(GameId gameId, GameAttemptResult attemptResult)
    {
        RoundResolutionResult resolution = attemptResult.Resolution;

        IReadOnlyDictionary<string, int>? scores = attemptResult.ScoreSnapshot is null
            ? null
            : attemptResult.ScoreSnapshot.Scores.ToDictionary(pair => pair.Key.Value.ToString(), pair => pair.Value);

        double? durationMilliseconds = resolution.ResolutionDuration?.TotalMilliseconds;

        return new RoundResultEvent(
            gameId.Value.ToString(),
            resolution.RoundResolved,
            resolution.AttemptAccepted,
            resolution.ResolvingPlayerId?.Value.ToString(),
            resolution.ResolvingPlayerCardIndex,
            resolution.MatchingSymbolId,
            resolution.RoundNumber,
            resolution.ProcessedAtUtc,
            durationMilliseconds,
            scores,
            attemptResult.GameCompleted);
    }

    private static GameOverEvent CreateGameOverEvent(GameId gameId, ScoreSnapshot snapshot)
    {
        IReadOnlyDictionary<string, int> scores = snapshot.Scores
            .ToDictionary(pair => pair.Key.Value.ToString(), pair => pair.Value);

        return new GameOverEvent(gameId.Value.ToString(), scores, snapshot.CapturedAtUtc);
    }
}
