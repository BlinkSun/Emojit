using EmojitServer.Domain.Enums;

namespace EmojitServer.Application.Contracts.Realtime;

/// <summary>
/// Represents the payload returned to the caller after a game is scheduled.
/// </summary>
public sealed class GameCreatedResponse
{
    /// <summary>
    /// Gets or sets the identifier of the newly created game session.
    /// </summary>
    public string GameId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the gameplay mode scheduled for the session.
    /// </summary>
    public GameMode Mode { get; init; }

    /// <summary>
    /// Gets or sets the maximum number of players allowed to join.
    /// </summary>
    public int MaxPlayers { get; init; }

    /// <summary>
    /// Gets or sets the maximum number of rounds configured for the session.
    /// </summary>
    public int MaxRounds { get; init; }
}
