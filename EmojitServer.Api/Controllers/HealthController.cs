using System;
using System.Threading;
using System.Threading.Tasks;
using EmojitServer.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EmojitServer.Api.Controllers;

/// <summary>
/// Provides a lightweight health probe endpoint for readiness checks.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Returns the current health status of the API host.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A health status payload.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(HealthStatusResponse), StatusCodes.Status200OK)]
    public ActionResult<HealthStatusResponse> Get(CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            HealthStatusResponse response = new("Healthy", DateTimeOffset.UtcNow);
            return Ok(response);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Health check request was canceled by the client.");
            return StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
    }
}
