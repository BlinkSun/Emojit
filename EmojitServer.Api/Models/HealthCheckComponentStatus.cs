using System;

namespace EmojitServer.Api.Models;

/// <summary>
/// Represents the outcome of an individual health check component.
/// </summary>
/// <param name="Status">The resulting status description.</param>
/// <param name="Description">An optional diagnostic description for the component.</param>
/// <param name="Duration">The time taken to execute the health check.</param>
public sealed record HealthCheckComponentStatus(string Status, string? Description, TimeSpan Duration);
