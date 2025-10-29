using System.Threading;
using System.Threading.Tasks;
using EmojitServer.Application.Services.Models;
using EmojitServer.Core.GameModes;
using EmojitServer.Domain.Entities;
using EmojitServer.Domain.Enums;
using EmojitServer.Domain.ValueObjects;

namespace EmojitServer.Application.Abstractions.Services;

/// <summary>
/// Provides high level orchestration for running Emojit games.
/// </summary>
public interface IGameService
{
    /// <summary>
    /// Schedules a new game session with the provided parameters.
    /// </summary>
    /// <param name="mode">The gameplay mode to schedule.</param>
    /// <param name="maxPlayers">The maximum number of allowed participants.</param>
    /// <param name="maxRounds">The maximum number of rounds to play.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The scheduled game session.</returns>
    Task<GameSession> CreateGameAsync(GameMode mode, int maxPlayers, int maxRounds, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a player to an existing scheduled game session.
    /// </summary>
    /// <param name="gameId">The identifier of the game session.</param>
    /// <param name="playerId">The identifier of the player joining the session.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task JoinGameAsync(GameId gameId, PlayerId playerId, CancellationToken cancellationToken);

    /// <summary>
    /// Starts a scheduled game session and produces the first round state.
    /// </summary>
    /// <param name="gameId">The identifier of the session to start.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The newly started round.</returns>
    Task<GameRoundState> StartGameAsync(GameId gameId, CancellationToken cancellationToken);

    /// <summary>
    /// Registers a symbol selection attempt for the specified player.
    /// </summary>
    /// <param name="gameId">The identifier of the session handling the attempt.</param>
    /// <param name="playerId">The identifier of the player submitting the attempt.</param>
    /// <param name="symbolId">The identifier of the selected symbol.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The processed attempt result along with contextual information.</returns>
    Task<GameAttemptResult> ClickSymbolAsync(GameId gameId, PlayerId playerId, int symbolId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the current score snapshot of the session.
    /// </summary>
    /// <param name="gameId">The identifier of the session.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current score snapshot.</returns>
    Task<ScoreSnapshot> GetScoresSnapshotAsync(GameId gameId, CancellationToken cancellationToken);

    /// <summary>
    /// Persists the endgame results and finalizes the session.
    /// </summary>
    /// <param name="gameId">The identifier of the game session.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated game session.</returns>
    Task<GameSession> PersistEndGameAsync(GameId gameId, CancellationToken cancellationToken);
}
