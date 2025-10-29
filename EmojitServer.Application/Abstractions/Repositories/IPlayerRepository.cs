using System.Threading;
using System.Threading.Tasks;
using EmojitServer.Domain.Entities;
using EmojitServer.Domain.ValueObjects;

namespace EmojitServer.Application.Abstractions.Repositories;

/// <summary>
/// Provides access to persisted <see cref="Player"/> aggregates.
/// </summary>
public interface IPlayerRepository
{
    /// <summary>
    /// Retrieves a player by identifier.
    /// </summary>
    /// <param name="playerId">The identifier of the player to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The player when found; otherwise, <c>null</c>.</returns>
    Task<Player?> GetByIdAsync(PlayerId playerId, CancellationToken cancellationToken);

    /// <summary>
    /// Persists a new player aggregate.
    /// </summary>
    /// <param name="player">The player to persist.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(Player player, CancellationToken cancellationToken);

    /// <summary>
    /// Persists updates made to an existing player aggregate.
    /// </summary>
    /// <param name="player">The player to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateAsync(Player player, CancellationToken cancellationToken);
}
