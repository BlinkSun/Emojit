using System;
using EmojitServer.Api.Models;
using EmojitServer.Core.Design;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EmojitServer.Api.Controllers;

/// <summary>
/// Provides informational endpoints exposing gameplay statistics and deterministic deck insights.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class StatsController : ControllerBase
{
    private readonly ILogger<StatsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatsController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public StatsController(ILogger<StatsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Retrieves statistics about the deterministic Emojit design for the specified order.
    /// </summary>
    /// <param name="order">The prime number representing the design order.</param>
    /// <returns>A description of the resulting deck layout.</returns>
    [HttpGet("design")]
    [ProducesResponseType(typeof(DesignStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<DesignStatsResponse> GetDesignStats([FromQuery] int order = 7)
    {
        try
        {
            EmojitDesign design = EmojitDesign.Create(order);
            EmojitDesignStats stats = design.GetStats();

            DesignStatsResponse response = new(stats.Order, stats.CardCount, stats.SymbolCount, stats.SymbolsPerCard);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid design order requested: {Order}.", order);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Unexpected error while computing design statistics for order {Order}.", order);
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to compute design statistics.");
        }
    }
}
