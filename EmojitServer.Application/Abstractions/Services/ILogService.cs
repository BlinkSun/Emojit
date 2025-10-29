using EmojitServer.Core.GameModes;
using EmojitServer.Domain.Entities;
using EmojitServer.Domain.ValueObjects;

namespace EmojitServer.Application.Abstractions.Services;

/// <summary>
/// Provides operations related to round logging and retrieval.
/// </summary>
public interface ILogService
{
    /// <summary>
    /// Creates a round log instance based on the supplied runtime information.
    /// </summary>
    /// <param name="session">The game session associated with the log.</param>
    /// <param name="roundState">The state of the round being logged.</param>
    /// <param name="resolution">The resolution result that concluded the round.</param>
    /// <returns>A fully populated round log entity.</returns>
    RoundLog CreateRoundLog(GameSession session, GameRoundState roundState, RoundResolutionResult resolution);

    /// <summary>
    /// Persists the provided round log.
    /// </summary>
    /// <param name="roundLog">The round log to persist.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task PersistRoundLogAsync(RoundLog roundLog, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the ordered round logs associated with the specified game session.
    /// </summary>
    /// <param name="gameId">The identifier of the game session.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ordered list of round logs.</returns>
    Task<IReadOnlyList<RoundLog>> GetLogsAsync(GameId gameId, CancellationToken cancellationToken);
}
