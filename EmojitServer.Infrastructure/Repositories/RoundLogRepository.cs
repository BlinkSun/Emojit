using EmojitServer.Application.Abstractions.Repositories;
using EmojitServer.Common.Exceptions;
using EmojitServer.Domain.Entities;
using EmojitServer.Domain.ValueObjects;
using EmojitServer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmojitServer.Infrastructure.Repositories;

/// <summary>
/// Provides Entity Framework Core backed access to <see cref="RoundLog"/> entities.
/// </summary>
public sealed class RoundLogRepository : IRoundLogRepository
{
    private readonly EmojitDbContext _dbContext;
    private readonly ILogger<RoundLogRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoundLogRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public RoundLogRepository(EmojitDbContext dbContext, ILogger<RoundLogRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task AddAsync(RoundLog roundLog, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(roundLog);

        try
        {
            await _dbContext.RoundLogs.AddAsync(roundLog, cancellationToken).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist round log {RoundLogId}.", roundLog.Id);
            throw new RepositoryOperationException($"Failed to persist round log '{roundLog.Id}'.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RoundLog>> GetByGameIdAsync(GameId gameId, CancellationToken cancellationToken)
    {
        try
        {
            List<RoundLog> logs = await _dbContext.RoundLogs
                .Where(log => log.GameId == gameId)
                .OrderBy(log => log.RoundNumber)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return logs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve round logs for game {GameId}.", gameId);
            throw new RepositoryOperationException($"Failed to retrieve round logs for game '{gameId}'.", ex);
        }
    }
}
