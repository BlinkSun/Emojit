using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EmojitServer.Api.Models;
using EmojitServer.Application.Abstractions.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EmojitServer.Api.Controllers;

/// <summary>
/// Exposes read-only endpoints for leaderboard data.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _leaderboardService;
    private readonly ILogger<LeaderboardController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LeaderboardController"/> class.
    /// </summary>
    /// <param name="leaderboardService">The leaderboard service handling retrieval.</param>
    /// <param name="logger">The logger instance.</param>
    public LeaderboardController(ILeaderboardService leaderboardService, ILogger<LeaderboardController> logger)
    {
        _leaderboardService = leaderboardService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the top leaderboard entries ordered by total points.
    /// </summary>
    /// <param name="count">The maximum number of entries to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of leaderboard entries.</returns>
    [HttpGet("top")]
    [ProducesResponseType(typeof(IReadOnlyCollection<LeaderboardEntryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyCollection<LeaderboardEntryResponse>>> GetTopAsync(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        if (count <= 0)
        {
            return BadRequest("Count must be greater than zero.");
        }

        try
        {
            IReadOnlyList<Domain.Entities.LeaderboardEntry> entries = await _leaderboardService
                .GetTopAsync(count, cancellationToken)
                .ConfigureAwait(false);

            List<LeaderboardEntryResponse> response = entries
                .Select(entry => new LeaderboardEntryResponse(
                    entry.PlayerId.Value,
                    entry.TotalPoints,
                    entry.GamesPlayed,
                    entry.GamesWon,
                    entry.LastUpdatedAtUtc))
                .ToList();

            return Ok(response);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _logger.LogWarning(ex, "Invalid count supplied to leaderboard endpoint: {Count}.", count);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to retrieve leaderboard entries.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to retrieve leaderboard entries.");
        }
    }
}
