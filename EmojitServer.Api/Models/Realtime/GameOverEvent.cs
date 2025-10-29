using System;
using System.Collections.Generic;

namespace EmojitServer.Api.Models.Realtime;

/// <summary>
/// Represents the payload broadcast when a game session finishes.
/// </summary>
public sealed class GameOverEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameOverEvent"/> class.
    /// </summary>
    /// <param name="gameId">The identifier of the completed game session.</param>
    /// <param name="finalScores">The final scores of all participants.</param>
    /// <param name="completedAtUtc">The timestamp when the game completed in UTC.</param>
    public GameOverEvent(string gameId, IReadOnlyDictionary<string, int> finalScores, DateTimeOffset completedAtUtc)
    {
        GameId = gameId;
        FinalScores = finalScores;
        CompletedAtUtc = completedAtUtc;
    }

    /// <summary>
    /// Gets the identifier of the completed game session.
    /// </summary>
    public string GameId { get; }

    /// <summary>
    /// Gets the final scores of all participants.
    /// </summary>
    public IReadOnlyDictionary<string, int> FinalScores { get; }

    /// <summary>
    /// Gets the timestamp when the game completed in UTC.
    /// </summary>
    public DateTimeOffset CompletedAtUtc { get; }
}
