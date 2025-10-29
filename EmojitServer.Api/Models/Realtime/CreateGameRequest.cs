using System;
using EmojitServer.Domain.Enums;

namespace EmojitServer.Api.Models.Realtime;

/// <summary>
/// Represents the payload sent by a client to schedule a new Emojit game.
/// </summary>
public sealed class CreateGameRequest
{
    /// <summary>
    /// Gets or sets the gameplay mode the requester wants to host.
    /// </summary>
    public GameMode Mode { get; set; } = GameMode.Tower;

    /// <summary>
    /// Gets or sets the maximum number of players allowed to join the session.
    /// </summary>
    public int MaxPlayers { get; set; } = 4;

    /// <summary>
    /// Gets or sets the maximum number of rounds to play before the match finishes.
    /// </summary>
    public int MaxRounds { get; set; } = 10;
}
