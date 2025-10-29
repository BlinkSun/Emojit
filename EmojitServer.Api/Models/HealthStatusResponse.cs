using System;

namespace EmojitServer.Api.Models;

/// <summary>
/// Represents a standard health response payload.
/// </summary>
/// <param name="Status">The textual status describing the system health.</param>
/// <param name="CheckedAtUtc">The timestamp when the health was evaluated in UTC.</param>
public sealed record HealthStatusResponse(string Status, DateTimeOffset CheckedAtUtc);
