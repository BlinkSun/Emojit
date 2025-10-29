using EmojitClient.Maui.Models.Authentication;

namespace EmojitClient.Maui.Abstractions.Authentication;

/// <summary>
/// Defines the contract for requesting authentication tokens from the Emojit server.
/// </summary>
public interface IAuthenticationApiClient
{
    /// <summary>
    /// Requests a JWT access token for the specified player credentials.
    /// </summary>
    /// <param name="request">The request payload containing player credentials.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The issued token response.</returns>
    Task<TokenResponse> RequestTokenAsync(TokenRequest request, CancellationToken cancellationToken = default);
}
