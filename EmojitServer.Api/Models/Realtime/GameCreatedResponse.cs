using System;
using EmojitServer.Domain.Enums;

namespace EmojitServer.Api.Models.Realtime;

/// <summary>
/// Represents the payload returned to the caller after a game is scheduled.
/// </summary>
public sealed class GameCreatedResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameCreatedResponse"/> class.
    /// </summary>
    /// <param name="gameId">The identifier of the newly created game session.</param>
    /// <param name="mode">The gameplay mode scheduled for the session.</param>
    /// <param name="maxPlayers">The maximum number of players allowed to join.</param>
    /// <param name="maxRounds">The maximum number of rounds configured.</param>
    public GameCreatedResponse(string gameId, GameMode mode, int maxPlayers, int maxRounds)
    {
        GameId = gameId;
        Mode = mode;
        MaxPlayers = maxPlayers;
        MaxRounds = maxRounds;
    }

    /// <summary>
    /// Gets the identifier of the newly created game session.
    /// </summary>
    public string GameId { get; }

    /// <summary>
    /// Gets the gameplay mode scheduled for the session.
    /// </summary>
    public GameMode Mode { get; }

    /// <summary>
    /// Gets the maximum number of players allowed to join.
    /// </summary>
    public int MaxPlayers { get; }

    /// <summary>
    /// Gets the maximum number of rounds configured.
    /// </summary>
    public int MaxRounds { get; }
}
