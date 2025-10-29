using EmojitServer.Application.Services.Models;
using EmojitServer.Domain.ValueObjects;

namespace EmojitServer.Application.Abstractions.Services;

/// <summary>
/// Defines operations for authenticating players and issuing access tokens.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates the specified player and returns an access token when successful.
    /// </summary>
    /// <param name="playerId">The identifier of the player requesting access.</param>
    /// <param name="displayName">The display name expected to match the stored player profile.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The authentication result containing the issued token.</returns>
    Task<AuthenticationResult> AuthenticatePlayerAsync(PlayerId playerId, string displayName, CancellationToken cancellationToken);
}
