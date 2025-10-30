using EmojitClient.Maui.Framework.Models.Leaderboard;

namespace EmojitClient.Maui.Framework.Abstractions.Leaderboard;

/// <summary>
/// Defines the operations required to query leaderboard data from the Emojit server.
/// </summary>
public interface ILeaderboardApiClient
{
    /// <summary>
    /// Retrieves the top leaderboard entries ordered by score.
    /// </summary>
    /// <param name="count">The maximum number of entries to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The requested collection of leaderboard entries.</returns>
    Task<IReadOnlyList<LeaderboardEntry>> GetTopEntriesAsync(int count = 10, CancellationToken cancellationToken = default);
}
