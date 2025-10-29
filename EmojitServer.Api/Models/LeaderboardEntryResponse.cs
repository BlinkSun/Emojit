using System;

namespace EmojitServer.Api.Models;

/// <summary>
/// Represents the payload returned for leaderboard entries.
/// </summary>
/// <param name="PlayerId">The unique identifier of the player.</param>
/// <param name="TotalPoints">The cumulative number of points earned by the player.</param>
/// <param name="GamesPlayed">The total number of games played.</param>
/// <param name="GamesWon">The total number of games won.</param>
/// <param name="LastUpdatedAtUtc">The timestamp in UTC when the entry was last updated.</param>
public sealed record LeaderboardEntryResponse(Guid PlayerId, int TotalPoints, int GamesPlayed, int GamesWon, DateTimeOffset LastUpdatedAtUtc);
