using System;

namespace EmojitServer.Api.Models.Realtime;

/// <summary>
/// Represents the payload sent by a client when attempting to join an existing game session.
/// </summary>
public sealed class JoinGameRequest
{
    /// <summary>
    /// Gets or sets the identifier of the game to join.
    /// </summary>
    public string GameId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the player requesting to join.
    /// </summary>
    public string PlayerId { get; set; } = string.Empty;
}
