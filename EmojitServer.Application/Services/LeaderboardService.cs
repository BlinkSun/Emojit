using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EmojitServer.Application.Abstractions.Repositories;
using EmojitServer.Application.Abstractions.Services;
using EmojitServer.Core.GameModes;
using EmojitServer.Domain.Entities;
using EmojitServer.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace EmojitServer.Application.Services;

/// <summary>
/// Handles leaderboard orchestration based on gameplay outcomes.
/// </summary>
public sealed class LeaderboardService : ILeaderboardService
{
    private readonly ILeaderboardRepository _leaderboardRepository;
    private readonly ILogger<LeaderboardService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LeaderboardService"/> class.
    /// </summary>
    /// <param name="leaderboardRepository">The repository used to persist leaderboard entries.</param>
    /// <param name="logger">The logger instance.</param>
    public LeaderboardService(ILeaderboardRepository leaderboardRepository, ILogger<LeaderboardService> logger)
    {
        _leaderboardRepository = leaderboardRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task UpdateLeaderboardAsync(GameSession session, ScoreSnapshot scores, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(scores);

        if (scores.Scores.Count == 0)
        {
            return;
        }

        int highestScore = scores.Scores.Values.Max();
        HashSet<PlayerId> winners = scores.Scores
            .Where(pair => pair.Value == highestScore)
            .Select(pair => pair.Key)
            .ToHashSet();

        DateTimeOffset timestamp = scores.CapturedAtUtc;

        foreach (KeyValuePair<PlayerId, int> entry in scores.Scores)
        {
            PlayerId playerId = entry.Key;
            int points = entry.Value;
            bool won = winners.Contains(playerId);

            try
            {
                LeaderboardEntry? existing = await _leaderboardRepository
                    .GetByPlayerIdAsync(playerId, cancellationToken)
                    .ConfigureAwait(false);

                if (existing is null)
                {
                    existing = LeaderboardEntry.Create(playerId, points, 1, won ? 1 : 0, timestamp);
                }
                else
                {
                    existing.ApplyMatchResult(points, won, timestamp);
                }

                await _leaderboardRepository.UpsertAsync(existing, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update leaderboard for player {PlayerId}.", playerId);
                throw new InvalidOperationException("Failed to update leaderboard due to an unexpected error.", ex);
            }
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LeaderboardEntry>> GetTopAsync(int count, CancellationToken cancellationToken)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Count cannot be negative.");
        }

        try
        {
            return await _leaderboardRepository.GetTopAsync(count, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve leaderboard entries.");
            throw new InvalidOperationException("Failed to retrieve leaderboard entries due to an unexpected error.", ex);
        }
    }
}
