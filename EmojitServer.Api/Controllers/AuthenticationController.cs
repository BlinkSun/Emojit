using System;
using System.Threading;
using System.Threading.Tasks;
using EmojitServer.Api.Models.Authentication;
using EmojitServer.Application.Abstractions.Services;
using EmojitServer.Application.Services.Models;
using EmojitServer.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EmojitServer.Api.Controllers;

/// <summary>
/// Provides endpoints used to issue authentication tokens for players.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthenticationController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationController"/> class.
    /// </summary>
    /// <param name="authenticationService">The authentication service issuing tokens.</param>
    /// <param name="logger">The logger instance.</param>
    public AuthenticationController(IAuthenticationService authenticationService, ILogger<AuthenticationController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    /// <summary>
    /// Issues a JWT token for the specified player when the credentials match.
    /// </summary>
    /// <param name="request">The authentication payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The issued token descriptor.</returns>
    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TokenResponse>> IssueTokenAsync(
        [FromBody] TokenRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return BadRequest("A request payload must be provided.");
        }

        if (!Guid.TryParse(request.PlayerId, out Guid rawPlayerId) || !PlayerId.TryFromGuid(rawPlayerId, out PlayerId playerId))
        {
            return BadRequest("A valid player identifier must be supplied.");
        }

        try
        {
            AuthenticationResult result = await _authenticationService
                .AuthenticatePlayerAsync(playerId, request.DisplayName, cancellationToken)
                .ConfigureAwait(false);

            TokenResponse response = new()
            {
                AccessToken = result.AccessToken,
                ExpiresAtUtc = result.ExpiresAtUtc,
                TokenType = "Bearer",
            };

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to authenticate player {PlayerId}.", request.PlayerId);
            return BadRequest(ex.Message);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while issuing token for player {PlayerId}.", request.PlayerId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to issue an authentication token at this time.");
        }
    }
}
