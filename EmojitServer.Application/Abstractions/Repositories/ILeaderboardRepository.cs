using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EmojitServer.Domain.Entities;
using EmojitServer.Domain.ValueObjects;

namespace EmojitServer.Application.Abstractions.Repositories;

/// <summary>
/// Provides persistence operations for <see cref="LeaderboardEntry"/> aggregates.
/// </summary>
public interface ILeaderboardRepository
{
    /// <summary>
    /// Retrieves a leaderboard entry for a specific player.
    /// </summary>
    /// <param name="playerId">The identifier of the player.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The leaderboard entry when present; otherwise, <c>null</c>.</returns>
    Task<LeaderboardEntry?> GetByPlayerIdAsync(PlayerId playerId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates or updates the provided leaderboard entry atomically.
    /// </summary>
    /// <param name="entry">The leaderboard entry to upsert.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpsertAsync(LeaderboardEntry entry, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the top entries ordered by total points.
    /// </summary>
    /// <param name="count">The maximum number of entries to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A read-only list of leaderboard entries.</returns>
    Task<IReadOnlyList<LeaderboardEntry>> GetTopAsync(int count, CancellationToken cancellationToken);
}
