using EmojitServer.Domain.Entities;
using EmojitServer.Domain.ValueObjects;

namespace EmojitServer.Application.Abstractions.Services;

/// <summary>
/// Encapsulates reusable validation routines applied across the application services.
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Ensures that the requested player exists.
    /// </summary>
    /// <param name="playerId">The identifier of the player.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resolved player entity.</returns>
    Task<Player> EnsurePlayerExistsAsync(PlayerId playerId, CancellationToken cancellationToken);

    /// <summary>
    /// Validates that the player can join the provided game session.
    /// </summary>
    /// <param name="session">The session the player wants to join.</param>
    /// <param name="playerId">The identifier of the player.</param>
    void EnsurePlayerCanJoin(GameSession session, PlayerId playerId);

    /// <summary>
    /// Validates that the session can transition to the started state.
    /// </summary>
    /// <param name="session">The session to validate.</param>
    void EnsureSessionCanStart(GameSession session);

    /// <summary>
    /// Validates that a session is currently active and accepting gameplay events.
    /// </summary>
    /// <param name="session">The session to validate.</param>
    void EnsureSessionIsActive(GameSession session);

    /// <summary>
    /// Validates that the player is allowed to submit attempts in the session.
    /// </summary>
    /// <param name="session">The session the player participates in.</param>
    /// <param name="playerId">The player identifier.</param>
    void EnsureAttemptAllowed(GameSession session, PlayerId playerId);
}
