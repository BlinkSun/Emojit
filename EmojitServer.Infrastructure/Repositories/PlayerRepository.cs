using EmojitServer.Application.Abstractions.Repositories;
using EmojitServer.Common.Exceptions;
using EmojitServer.Domain.Entities;
using EmojitServer.Domain.ValueObjects;
using EmojitServer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmojitServer.Infrastructure.Repositories;

/// <summary>
/// Provides Entity Framework Core backed access to <see cref="Player"/> entities.
/// </summary>
public sealed class PlayerRepository : IPlayerRepository
{
    private readonly EmojitDbContext _dbContext;
    private readonly ILogger<PlayerRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public PlayerRepository(EmojitDbContext dbContext, ILogger<PlayerRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Player?> GetByIdAsync(PlayerId playerId, CancellationToken cancellationToken)
    {
        try
        {
            return await _dbContext.Players
                .AsNoTracking()
                .FirstOrDefaultAsync(player => player.Id == playerId, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve player {PlayerId}.", playerId);
            throw new RepositoryOperationException($"Failed to retrieve player '{playerId}'.", ex);
        }
    }

    /// <inheritdoc />
    public async Task AddAsync(Player player, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(player);

        try
        {
            await _dbContext.Players.AddAsync(player, cancellationToken).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist player {PlayerId}.", player.Id);
            throw new RepositoryOperationException($"Failed to persist player '{player.Id}'.", ex);
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Player player, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(player);

        try
        {
            _dbContext.Players.Update(player);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update player {PlayerId}.", player.Id);
            throw new RepositoryOperationException($"Failed to update player '{player.Id}'.", ex);
        }
    }
}
