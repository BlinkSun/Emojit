using EmojitServer.Core.GameModes;
using EmojitServer.Domain.Entities;

namespace EmojitServer.Application.Abstractions.Services;

/// <summary>
/// Provides leaderboard related operations.
/// </summary>
public interface ILeaderboardService
{
    /// <summary>
    /// Applies the outcome of a completed game to the leaderboard entries of the participants.
    /// </summary>
    /// <param name="session">The completed game session.</param>
    /// <param name="scores">The final score snapshot of the session.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateLeaderboardAsync(GameSession session, ScoreSnapshot scores, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the top entries of the leaderboard ordered by accumulated points.
    /// </summary>
    /// <param name="count">The maximum number of entries to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ordered leaderboard entries.</returns>
    Task<IReadOnlyList<LeaderboardEntry>> GetTopAsync(int count, CancellationToken cancellationToken);
}
