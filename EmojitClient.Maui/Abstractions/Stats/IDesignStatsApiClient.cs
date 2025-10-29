using EmojitClient.Maui.Models.Stats;

namespace EmojitClient.Maui.Abstractions.Stats;

/// <summary>
/// Defines operations used to retrieve deterministic deck statistics from the Emojit server.
/// </summary>
public interface IDesignStatsApiClient
{
    /// <summary>
    /// Retrieves design statistics for the specified order.
    /// </summary>
    /// <param name="order">The design order (prime number) requested.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The corresponding design statistics.</returns>
    Task<DesignStats> GetDesignStatsAsync(int order = 7, CancellationToken cancellationToken = default);
}
