using EmojitServer.Domain.Entities;
using EmojitServer.Domain.ValueObjects;

namespace EmojitServer.Application.Abstractions.Repositories;

/// <summary>
/// Provides access to persisted <see cref="RoundLog"/> records.
/// </summary>
public interface IRoundLogRepository
{
    /// <summary>
    /// Persists a new round log.
    /// </summary>
    /// <param name="roundLog">The round log to persist.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(RoundLog roundLog, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the ordered round logs associated with a game session.
    /// </summary>
    /// <param name="gameId">The identifier of the game session.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ordered collection of round logs.</returns>
    Task<IReadOnlyList<RoundLog>> GetByGameIdAsync(GameId gameId, CancellationToken cancellationToken);
}
