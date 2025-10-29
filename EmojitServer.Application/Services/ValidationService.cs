using EmojitServer.Application.Abstractions.Repositories;
using EmojitServer.Application.Abstractions.Services;
using EmojitServer.Domain.Entities;
using EmojitServer.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace EmojitServer.Application.Services;

/// <summary>
/// Provides reusable validation routines for application services.
/// </summary>
public sealed class ValidationService : IValidationService
{
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<ValidationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationService"/> class.
    /// </summary>
    /// <param name="playerRepository">The player repository.</param>
    /// <param name="logger">The logger instance.</param>
    public ValidationService(IPlayerRepository playerRepository, ILogger<ValidationService> logger)
    {
        _playerRepository = playerRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Player> EnsurePlayerExistsAsync(PlayerId playerId, CancellationToken cancellationToken)
    {
        if (playerId.IsEmpty)
        {
            throw new ArgumentException("Player identifier must be provided.", nameof(playerId));
        }

        try
        {
            Player? player = await _playerRepository.GetByIdAsync(playerId, cancellationToken).ConfigureAwait(false);
            if (player is null)
            {
                throw new InvalidOperationException($"Player '{playerId}' does not exist.");
            }

            return player;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate player {PlayerId} existence.", playerId);
            throw new InvalidOperationException("Failed to validate player existence due to an unexpected error.", ex);
        }
    }

    /// <inheritdoc />
    public void EnsurePlayerCanJoin(GameSession session, PlayerId playerId)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (playerId.IsEmpty)
        {
            throw new ArgumentException("Player identifier must be provided.", nameof(playerId));
        }

        if (session.IsStarted)
        {
            throw new InvalidOperationException("The session already started and cannot accept new players.");
        }

        if (session.IsCompleted)
        {
            throw new InvalidOperationException("The session already completed.");
        }

        if (session.Participants.Contains(playerId))
        {
            throw new InvalidOperationException("The player already joined the session.");
        }

        if (session.Participants.Count >= session.MaxPlayers)
        {
            throw new InvalidOperationException("The session reached its maximum capacity.");
        }
    }

    /// <inheritdoc />
    public void EnsureSessionCanStart(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (session.IsStarted)
        {
            throw new InvalidOperationException("The session has already started.");
        }

        if (session.IsCompleted)
        {
            throw new InvalidOperationException("The session has already completed.");
        }

        if (session.Participants.Count < 2)
        {
            throw new InvalidOperationException("At least two players are required to start the session.");
        }
    }

    /// <inheritdoc />
    public void EnsureSessionIsActive(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (!session.IsStarted)
        {
            throw new InvalidOperationException("The session has not started yet.");
        }

        if (session.IsCompleted)
        {
            throw new InvalidOperationException("The session already completed.");
        }
    }

    /// <inheritdoc />
    public void EnsureAttemptAllowed(GameSession session, PlayerId playerId)
    {
        EnsureSessionIsActive(session);

        if (playerId.IsEmpty)
        {
            throw new ArgumentException("Player identifier must be provided.", nameof(playerId));
        }

        if (!session.Participants.Any(participant => participant == playerId))
        {
            throw new InvalidOperationException("The player is not registered in the session.");
        }
    }
}
