using EmojitClient.Maui.Framework.Abstractions.Realtime;
using EmojitClient.Maui.Framework.Exceptions;
using EmojitClient.Maui.Framework.Models.Realtime;
using EmojitClient.Maui.Framework.Options;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmojitClient.Maui.Framework.Services.Realtime;

/// <summary>
/// Provides a strongly typed wrapper around the Emojit real-time SignalR hub.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GameHubClient"/> class.
/// </remarks>
/// <param name="optionsAccessor">Provides access to the configured API options.</param>
/// <param name="logger">The logger instance.</param>
public sealed class GameHubClient(IOptions<EmojitApiOptions> optionsAccessor, ILogger<GameHubClient> logger) : IGameHubClient
{
    private const string CreateGameMethod = "CreateGame";
    private const string JoinGameMethod = "JoinGame";
    private const string StartGameMethod = "StartGame";
    private const string ClickSymbolMethod = "ClickSymbol";
    private const string RoundStartMessage = "RoundStart";
    private const string RoundResultMessage = "RoundResult";
    private const string GameOverMessage = "GameOver";

    private readonly EmojitApiOptions options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
    private readonly ILogger<GameHubClient> logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly SemaphoreSlim connectionLock = new(1, 1);

    private HubConnection? _hubConnection;
    private Func<CancellationToken, Task<string>>? _accessTokenProvider;

    /// <inheritdoc />
    public event Func<RoundStartEvent, Task>? RoundStarted;

    /// <inheritdoc />
    public event Func<RoundResultEvent, Task>? RoundResultReceived;

    /// <inheritdoc />
    public event Func<GameOverEvent, Task>? GameCompleted;

    /// <inheritdoc />
    public async Task ConnectAsync(Func<CancellationToken, Task<string>> accessTokenProvider, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(accessTokenProvider);

        options.Validate();

        await connectionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _accessTokenProvider = accessTokenProvider;

            if (_hubConnection is not null)
            {
                switch (_hubConnection.State)
                {
                    case HubConnectionState.Connected:
                    case HubConnectionState.Connecting:
                    case HubConnectionState.Reconnecting:
                        logger.LogDebug("Reusing existing SignalR connection. State={State}.", _hubConnection.State);
                        return;
                }

                await _hubConnection.DisposeAsync().ConfigureAwait(false);
                _hubConnection = null;
            }

            _hubConnection = BuildHubConnection();
            RegisterHandlers(_hubConnection);

            await _hubConnection.StartAsync(cancellationToken).ConfigureAwait(false);
            logger.LogInformation("Connected to Emojit game hub at {HubUri}.", options.BuildUri(options.GameHubPath));
        }
        finally
        {
            connectionLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await connectionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_hubConnection is null)
            {
                return;
            }

            try
            {
                await _hubConnection.StopAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                await _hubConnection.DisposeAsync().ConfigureAwait(false);
                _hubConnection = null;
            }
        }
        finally
        {
            connectionLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<GameCreatedResponse> CreateGameAsync(CreateGameRequest request, CancellationToken cancellationToken = default)
    {
        HubConnection connection = await EnsureConnectionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            return await connection.InvokeAsync<GameCreatedResponse>(CreateGameMethod, request, cancellationToken).ConfigureAwait(false);
        }
        catch (HubException ex)
        {
            throw new EmojitRealtimeException(ex.Message, ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while invoking {Method}.", CreateGameMethod);
            throw new EmojitRealtimeException("An unexpected error occurred while creating the game.", ex);
        }
    }

    /// <inheritdoc />
    public async Task JoinGameAsync(JoinGameRequest request, CancellationToken cancellationToken = default)
    {
        HubConnection connection = await EnsureConnectionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await connection.InvokeAsync(JoinGameMethod, request, cancellationToken).ConfigureAwait(false);
        }
        catch (HubException ex)
        {
            throw new EmojitRealtimeException(ex.Message, ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while invoking {Method}.", JoinGameMethod);
            throw new EmojitRealtimeException("An unexpected error occurred while joining the game.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<RoundStartEvent> StartGameAsync(string gameId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(gameId))
        {
            throw new ArgumentException("A valid game identifier must be supplied.", nameof(gameId));
        }

        HubConnection connection = await EnsureConnectionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            return await connection.InvokeAsync<RoundStartEvent>(StartGameMethod, gameId, cancellationToken).ConfigureAwait(false);
        }
        catch (HubException ex)
        {
            throw new EmojitRealtimeException(ex.Message, ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while invoking {Method}.", StartGameMethod);
            throw new EmojitRealtimeException("An unexpected error occurred while starting the game.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<RoundResultEvent> ClickSymbolAsync(ClickSymbolRequest request, CancellationToken cancellationToken = default)
    {
        HubConnection connection = await EnsureConnectionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            return await connection.InvokeAsync<RoundResultEvent>(ClickSymbolMethod, request, cancellationToken).ConfigureAwait(false);
        }
        catch (HubException ex)
        {
            throw new EmojitRealtimeException(ex.Message, ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while invoking {Method}.", ClickSymbolMethod);
            throw new EmojitRealtimeException("An unexpected error occurred while submitting the attempt.", ex);
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync().ConfigureAwait(false);
        connectionLock.Dispose();
    }

    private HubConnection BuildHubConnection()
    {
        Uri hubUri = options.BuildUri(options.GameHubPath);

        HubConnection connection = new HubConnectionBuilder()
            .WithUrl(hubUri, options =>
            {
                options.AccessTokenProvider = async () =>
                {
                    if (_accessTokenProvider is null)
                    {
                        return string.Empty;
                    }

                    try
                    {
                        return await _accessTokenProvider(CancellationToken.None).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to retrieve access token for SignalR connection.");
                        throw;
                    }
                };
            })
            .WithAutomaticReconnect()
            .Build();

        connection.Closed += OnConnectionClosedAsync;
        connection.Reconnecting += OnConnectionReconnectingAsync;
        connection.Reconnected += OnConnectionReconnectedAsync;

        return connection;
    }

    private Task OnConnectionClosedAsync(Exception? exception)
    {
        if (exception is null)
        {
            logger.LogInformation("SignalR connection closed gracefully.");
        }
        else
        {
            logger.LogWarning(exception, "SignalR connection closed due to an error.");
        }

        return Task.CompletedTask;
    }

    private Task OnConnectionReconnectingAsync(Exception? exception)
    {
        if (exception is null)
        {
            logger.LogWarning("SignalR connection is reconnecting.");
        }
        else
        {
            logger.LogWarning(exception, "SignalR connection is reconnecting after an error.");
        }

        return Task.CompletedTask;
    }

    private Task OnConnectionReconnectedAsync(string? connectionId)
    {
        logger.LogInformation("SignalR connection re-established. ConnectionId={ConnectionId}.", connectionId);
        return Task.CompletedTask;
    }

    private void RegisterHandlers(HubConnection connection)
    {
        connection.Remove(RoundStartMessage);
        connection.Remove(RoundResultMessage);
        connection.Remove(GameOverMessage);

        connection.On<RoundStartEvent>(RoundStartMessage, payload => DispatchAsync(RoundStarted, payload, nameof(RoundStarted)));
        connection.On<RoundResultEvent>(RoundResultMessage, payload => DispatchAsync(RoundResultReceived, payload, nameof(RoundResultReceived)));
        connection.On<GameOverEvent>(GameOverMessage, payload => DispatchAsync(GameCompleted, payload, nameof(GameCompleted)));
    }

    private async Task DispatchAsync<T>(Func<T, Task>? handler, T payload, string eventName)
    {
        if (handler is null)
        {
            return;
        }

        foreach (Func<T, Task> subscriber in handler.GetInvocationList().Cast<Func<T, Task>>())
        {
            try
            {
                await subscriber(payload).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while executing {EventName} subscriber.", eventName);
            }
        }
    }

    private async Task<HubConnection> EnsureConnectionAsync(CancellationToken cancellationToken)
    {
        await connectionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_hubConnection is null)
            {
                throw new EmojitRealtimeException("The SignalR connection has not been established. Call ConnectAsync first.");
            }

            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                if (_accessTokenProvider is null)
                {
                    throw new EmojitRealtimeException("An access token provider is not configured.");
                }

                logger.LogInformation("Reconnecting to Emojit game hub.");

                await _hubConnection.StartAsync(cancellationToken).ConfigureAwait(false);
            }

            return _hubConnection;
        }
        finally
        {
            connectionLock.Release();
        }
    }
}
