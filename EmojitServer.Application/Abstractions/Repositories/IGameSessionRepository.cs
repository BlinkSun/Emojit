using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EmojitServer.Domain.Entities;
using EmojitServer.Domain.ValueObjects;

namespace EmojitServer.Application.Abstractions.Repositories;

/// <summary>
/// Provides persistence operations for <see cref="GameSession"/> aggregates.
/// </summary>
public interface IGameSessionRepository
{
    /// <summary>
    /// Retrieves a game session by its identifier.
    /// </summary>
    /// <param name="gameId">The identifier of the game session.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The requested game session when found; otherwise, <c>null</c>.</returns>
    Task<GameSession?> GetByIdAsync(GameId gameId, CancellationToken cancellationToken);

    /// <summary>
    /// Persists a newly scheduled game session.
    /// </summary>
    /// <param name="gameSession">The game session to persist.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(GameSession gameSession, CancellationToken cancellationToken);

    /// <summary>
    /// Persists updates made to a tracked game session.
    /// </summary>
    /// <param name="gameSession">The game session to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateAsync(GameSession gameSession, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves sessions that are currently active (started but not completed).
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A read-only collection of active game sessions.</returns>
    Task<IReadOnlyCollection<GameSession>> GetActiveSessionsAsync(CancellationToken cancellationToken);
}
