using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EmojitServer.Application.Abstractions.Repositories;
using EmojitServer.Common.Exceptions;
using EmojitServer.Domain.Entities;
using EmojitServer.Domain.ValueObjects;
using EmojitServer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmojitServer.Infrastructure.Repositories;

/// <summary>
/// Provides Entity Framework Core backed access to <see cref="GameSession"/> aggregates.
/// </summary>
public sealed class GameSessionRepository : IGameSessionRepository
{
    private readonly EmojitDbContext _dbContext;
    private readonly ILogger<GameSessionRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameSessionRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public GameSessionRepository(EmojitDbContext dbContext, ILogger<GameSessionRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<GameSession?> GetByIdAsync(GameId gameId, CancellationToken cancellationToken)
    {
        try
        {
            return await _dbContext.GameSessions
                .Include(session => session.RoundLogs)
                .FirstOrDefaultAsync(session => session.Id == gameId, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve game session {GameId}.", gameId);
            throw new RepositoryOperationException($"Failed to retrieve game session '{gameId}'.", ex);
        }
    }

    /// <inheritdoc />
    public async Task AddAsync(GameSession gameSession, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        try
        {
            await _dbContext.GameSessions.AddAsync(gameSession, cancellationToken).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist game session {GameId}.", gameSession.Id);
            throw new RepositoryOperationException($"Failed to persist game session '{gameSession.Id}'.", ex);
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(GameSession gameSession, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        try
        {
            _dbContext.GameSessions.Update(gameSession);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update game session {GameId}.", gameSession.Id);
            throw new RepositoryOperationException($"Failed to update game session '{gameSession.Id}'.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<GameSession>> GetActiveSessionsAsync(CancellationToken cancellationToken)
    {
        try
        {
            List<GameSession> sessions = await _dbContext.GameSessions
                .Where(session => session.IsStarted && !session.IsCompleted)
                .OrderBy(session => session.CreatedAtUtc)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return sessions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve active game sessions.");
            throw new RepositoryOperationException("Failed to retrieve active game sessions.", ex);
        }
    }
}
