using EmojitClient.Maui.Framework.Models.Realtime;

namespace EmojitClient.Maui.Framework.Abstractions.Realtime;

/// <summary>
/// Defines the operations and events exposed by the Emojit real-time game hub.
/// </summary>
public interface IGameHubClient : IAsyncDisposable
{
    /// <summary>
    /// Raised when the server broadcasts the start of a new round.
    /// </summary>
    event Func<RoundStartEvent, Task>? RoundStarted;

    /// <summary>
    /// Raised when the server broadcasts a round resolution.
    /// </summary>
    event Func<RoundResultEvent, Task>? RoundResultReceived;

    /// <summary>
    /// Raised when the server broadcasts that a game session has completed.
    /// </summary>
    event Func<GameOverEvent, Task>? GameCompleted;

    /// <summary>
    /// Establishes a SignalR connection to the game hub.
    /// </summary>
    /// <param name="accessTokenProvider">Provides an access token for authenticated connections.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ConnectAsync(Func<CancellationToken, Task<string>> accessTokenProvider, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gracefully shuts down the underlying SignalR connection.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests that the server schedule a new game session.
    /// </summary>
    /// <param name="request">The creation payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The description of the created session.</returns>
    Task<GameCreatedResponse> CreateGameAsync(CreateGameRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests to join an existing game session.
    /// </summary>
    /// <param name="request">The join payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task JoinGameAsync(JoinGameRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests that the server starts a scheduled game session.
    /// </summary>
    /// <param name="gameId">The identifier of the session to start.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The first round descriptor.</returns>
    Task<RoundStartEvent> StartGameAsync(string gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits a symbol attempt for the current round.
    /// </summary>
    /// <param name="request">The attempt payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resulting round resolution event.</returns>
    Task<RoundResultEvent> ClickSymbolAsync(ClickSymbolRequest request, CancellationToken cancellationToken = default);
}
