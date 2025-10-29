using EmojitServer.Application.Abstractions.Repositories;
using EmojitServer.Common.Exceptions;
using EmojitServer.Domain.Entities;
using EmojitServer.Domain.ValueObjects;
using EmojitServer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmojitServer.Infrastructure.Repositories;

/// <summary>
/// Provides Entity Framework Core backed access to <see cref="LeaderboardEntry"/> aggregates.
/// </summary>
public sealed class LeaderboardRepository : ILeaderboardRepository
{
    private readonly EmojitDbContext _dbContext;
    private readonly ILogger<LeaderboardRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LeaderboardRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public LeaderboardRepository(EmojitDbContext dbContext, ILogger<LeaderboardRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<LeaderboardEntry?> GetByPlayerIdAsync(PlayerId playerId, CancellationToken cancellationToken)
    {
        try
        {
            return await _dbContext.LeaderboardEntries
                .AsNoTracking()
                .FirstOrDefaultAsync(entry => entry.PlayerId == playerId, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve leaderboard entry for player {PlayerId}.", playerId);
            throw new RepositoryOperationException($"Failed to retrieve leaderboard entry for player '{playerId}'.", ex);
        }
    }

    /// <inheritdoc />
    public async Task UpsertAsync(LeaderboardEntry entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);

        try
        {
            bool exists = await _dbContext.LeaderboardEntries
                .AnyAsync(existing => existing.PlayerId == entry.PlayerId, cancellationToken)
                .ConfigureAwait(false);

            if (exists)
            {
                _dbContext.LeaderboardEntries.Update(entry);
            }
            else
            {
                await _dbContext.LeaderboardEntries.AddAsync(entry, cancellationToken).ConfigureAwait(false);
            }

            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert leaderboard entry for player {PlayerId}.", entry.PlayerId);
            throw new RepositoryOperationException($"Failed to upsert leaderboard entry for player '{entry.PlayerId}'.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LeaderboardEntry>> GetTopAsync(int count, CancellationToken cancellationToken)
    {
        try
        {
            List<LeaderboardEntry> entries = await _dbContext.LeaderboardEntries
                .OrderByDescending(entry => entry.TotalPoints)
                .ThenBy(entry => entry.LastUpdatedAtUtc)
                .Take(Math.Max(0, count))
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return entries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve leaderboard entries.");
            throw new RepositoryOperationException("Failed to retrieve leaderboard entries.", ex);
        }
    }
}
