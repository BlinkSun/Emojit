using EmojitServer.Core.GameModes;

namespace EmojitServer.Application.Services.Models;

/// <summary>
/// Represents the aggregated outcome of processing a symbol click during a game session.
/// </summary>
public sealed class GameAttemptResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameAttemptResult"/> class.
    /// </summary>
    /// <param name="resolution">The resolution result returned by the game mode.</param>
    /// <param name="nextRound">The next round state when the game continues.</param>
    /// <param name="gameCompleted">Indicates whether the game completed as a result of the attempt.</param>
    /// <param name="scoreSnapshot">The updated score snapshot captured after processing the attempt.</param>
    public GameAttemptResult(
        RoundResolutionResult resolution,
        GameRoundState? nextRound,
        bool gameCompleted,
        ScoreSnapshot? scoreSnapshot)
    {
        Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        NextRound = nextRound;
        GameCompleted = gameCompleted;
        ScoreSnapshot = scoreSnapshot;
    }

    /// <summary>
    /// Gets the resolution information returned by the game mode.
    /// </summary>
    public RoundResolutionResult Resolution { get; }

    /// <summary>
    /// Gets the next round state when a subsequent round is scheduled.
    /// </summary>
    public GameRoundState? NextRound { get; }

    /// <summary>
    /// Gets a value indicating whether the game completed after the attempt.
    /// </summary>
    public bool GameCompleted { get; }

    /// <summary>
    /// Gets the updated score snapshot captured after processing the attempt.
    /// </summary>
    public ScoreSnapshot? ScoreSnapshot { get; }
}
