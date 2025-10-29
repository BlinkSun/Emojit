using EmojitServer.Application.Abstractions.Repositories;
using EmojitServer.Application.Abstractions.Services;
using EmojitServer.Core.GameModes;
using EmojitServer.Domain.Entities;
using EmojitServer.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace EmojitServer.Application.Services;

/// <summary>
/// Provides functionality to materialize and persist round logs.
/// </summary>
public sealed class LogService : ILogService
{
    private readonly IRoundLogRepository _roundLogRepository;
    private readonly ILogger<LogService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogService"/> class.
    /// </summary>
    /// <param name="roundLogRepository">The repository responsible for persisting round logs.</param>
    /// <param name="logger">The logger instance.</param>
    public LogService(IRoundLogRepository roundLogRepository, ILogger<LogService> logger)
    {
        _roundLogRepository = roundLogRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public RoundLog CreateRoundLog(GameSession session, GameRoundState roundState, RoundResolutionResult resolution)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(roundState);
        ArgumentNullException.ThrowIfNull(resolution);

        if (!resolution.RoundResolved || !resolution.AttemptAccepted)
        {
            throw new InvalidOperationException("Round logs can only be created for resolved and accepted attempts.");
        }

        if (!resolution.RoundNumber.HasValue)
        {
            throw new InvalidOperationException("Resolved round results must include a round number.");
        }

        if (!resolution.MatchingSymbolId.HasValue)
        {
            throw new InvalidOperationException("Resolved round results must include the matching symbol identifier.");
        }

        PlayerId? winnerId = resolution.ResolvingPlayerId;
        int? winnerCardIndex = resolution.ResolvingPlayerCardIndex;

        if (winnerId.HasValue && !winnerCardIndex.HasValue && roundState.PlayerCardIndexes.TryGetValue(winnerId.Value, out int cardIndex))
        {
            winnerCardIndex = cardIndex;
        }

        TimeSpan resolutionDuration = resolution.ResolutionDuration ?? TimeSpan.Zero;

        return RoundLog.Create(
            session.Id,
            resolution.RoundNumber.Value,
            winnerId,
            roundState.SharedCardIndex,
            winnerCardIndex,
            resolution.MatchingSymbolId.Value,
            resolution.ProcessedAtUtc,
            resolutionDuration);
    }

    /// <inheritdoc />
    public async Task PersistRoundLogAsync(RoundLog roundLog, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(roundLog);

        try
        {
            await _roundLogRepository.AddAsync(roundLog, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist round log {RoundLogId}.", roundLog.Id);
            throw new InvalidOperationException("Failed to persist round log due to an unexpected error.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RoundLog>> GetLogsAsync(GameId gameId, CancellationToken cancellationToken)
    {
        if (gameId.IsEmpty)
        {
            throw new ArgumentException("Game identifier must be provided.", nameof(gameId));
        }

        try
        {
            return await _roundLogRepository.GetByGameIdAsync(gameId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve round logs for game {GameId}.", gameId);
            throw new InvalidOperationException("Failed to retrieve round logs due to an unexpected error.", ex);
        }
    }
}
