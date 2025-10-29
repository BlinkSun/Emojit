using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EmojitServer.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EmojitServer.Api.Controllers;

/// <summary>
/// Provides a lightweight health probe endpoint for readiness checks.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;
    private readonly HealthCheckService _healthCheckService;

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="healthCheckService">The health check service used to evaluate dependencies.</param>
    public HealthController(ILogger<HealthController> logger, HealthCheckService healthCheckService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(healthCheckService);

        _logger = logger;
        _healthCheckService = healthCheckService;
    }

    /// <summary>
    /// Returns the current health status of the API host.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A health status payload.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(HealthStatusResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<HealthStatusResponse>> Get(CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            HealthReport report = await _healthCheckService.CheckHealthAsync(cancellationToken);

            IReadOnlyDictionary<string, HealthCheckComponentStatus> components = report.Entries
                .ToDictionary(
                    entry => entry.Key,
                    entry => new HealthCheckComponentStatus(
                        entry.Value.Status.ToString(),
                        entry.Value.Description,
                        entry.Value.Duration));

            HealthStatusResponse response = new(
                report.Status.ToString(),
                DateTimeOffset.UtcNow,
                components);

            if (report.Status == HealthStatus.Healthy)
            {
                return Ok(response);
            }

            return StatusCode(StatusCodes.Status503ServiceUnavailable, response);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Health check request was canceled by the client.");
            return StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Health check evaluation failed due to an unexpected error.");

            HealthStatusResponse failureResponse = new(
                HealthStatus.Unhealthy.ToString(),
                DateTimeOffset.UtcNow,
                new Dictionary<string, HealthCheckComponentStatus>());

            return StatusCode(StatusCodes.Status500InternalServerError, failureResponse);
        }
    }
}
