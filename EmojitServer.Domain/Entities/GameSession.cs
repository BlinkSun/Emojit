using EmojitServer.Domain.Enums;
using EmojitServer.Domain.ValueObjects;

namespace EmojitServer.Domain.Entities;

/// <summary>
/// Represents a scheduled or running game session.
/// </summary>
public sealed class GameSession
{
    private readonly List<PlayerId> _participants = [];
    private readonly List<RoundLog> _roundLogs = [];

    private GameSession()
    {
        CreatedAtUtc = DateTimeOffset.UtcNow;
        LastUpdatedAtUtc = CreatedAtUtc;
    }

    private GameSession(GameId id, GameMode mode, int maxPlayers, int maxRounds, DateTimeOffset createdAtUtc)
    {
        Id = id;
        Mode = mode;
        MaxPlayers = maxPlayers;
        MaxRounds = maxRounds;
        CreatedAtUtc = createdAtUtc;
        LastUpdatedAtUtc = createdAtUtc;
    }

    /// <summary>
    /// Gets the unique identifier of the game session.
    /// </summary>
    public GameId Id { get; private set; }

    /// <summary>
    /// Gets the gameplay mode of the session.
    /// </summary>
    public GameMode Mode { get; private set; }

    /// <summary>
    /// Gets the maximum number of concurrent players allowed in the session.
    /// </summary>
    public int MaxPlayers { get; private set; }

    /// <summary>
    /// Gets the maximum number of rounds configured for the session.
    /// </summary>
    public int MaxRounds { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the session was created.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the session was last updated.
    /// </summary>
    public DateTimeOffset LastUpdatedAtUtc { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the session started, if it has started.
    /// </summary>
    public DateTimeOffset? StartedAtUtc { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the session completed, if it has completed.
    /// </summary>
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the session has started.
    /// </summary>
    public bool IsStarted => StartedAtUtc.HasValue;

    /// <summary>
    /// Gets a value indicating whether the session has completed.
    /// </summary>
    public bool IsCompleted => CompletedAtUtc.HasValue;

    /// <summary>
    /// Gets the read-only collection of participant identifiers.
    /// </summary>
    public IReadOnlyCollection<PlayerId> Participants => _participants.AsReadOnly();

    /// <summary>
    /// Gets the read-only collection of round logs associated with the session.
    /// </summary>
    public IReadOnlyCollection<RoundLog> RoundLogs => _roundLogs.AsReadOnly();

    /// <summary>
    /// Creates a new <see cref="GameSession"/> with the provided parameters.
    /// </summary>
    /// <param name="id">The unique identifier of the session.</param>
    /// <param name="mode">The gameplay mode for the session.</param>
    /// <param name="maxPlayers">The maximum number of players allowed.</param>
    /// <param name="maxRounds">The configured maximum number of rounds.</param>
    /// <param name="createdAtUtc">The creation timestamp in UTC.</param>
    /// <returns>A new <see cref="GameSession"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when any provided argument violates domain invariants.</exception>
    public static GameSession Schedule(GameId id, GameMode mode, int maxPlayers, int maxRounds, DateTimeOffset createdAtUtc)
    {
        if (id.IsEmpty)
        {
            throw new ArgumentException("Game identifier must be defined.", nameof(id));
        }

        if (!Enum.IsDefined(typeof(GameMode), mode))
        {
            throw new ArgumentException("Invalid game mode.", nameof(mode));
        }

        if (maxPlayers < 2)
        {
            throw new ArgumentException("At least two players are required for a session.", nameof(maxPlayers));
        }

        if (maxRounds <= 0)
        {
            throw new ArgumentException("A session must have at least one round.", nameof(maxRounds));
        }

        DateTimeOffset normalizedCreation = EnsureUtc(createdAtUtc);
        return new GameSession(id, mode, maxPlayers, maxRounds, normalizedCreation);
    }

    /// <summary>
    /// Adds a participant to the session while enforcing capacity constraints.
    /// </summary>
    /// <param name="playerId">The player identifier to add.</param>
    public void AddParticipant(PlayerId playerId)
    {
        if (playerId.IsEmpty)
        {
            throw new ArgumentException("Player identifier must be defined.", nameof(playerId));
        }

        if (_participants.Contains(playerId))
        {
            return;
        }

        if (_participants.Count >= MaxPlayers)
        {
            throw new InvalidOperationException("The session already reached its maximum capacity.");
        }

        _participants.Add(playerId);
        Touch(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Removes a participant from the session, if present.
    /// </summary>
    /// <param name="playerId">The player identifier to remove.</param>
    public void RemoveParticipant(PlayerId playerId)
    {
        if (playerId.IsEmpty)
        {
            throw new ArgumentException("Player identifier must be defined.", nameof(playerId));
        }

        if (_participants.Remove(playerId))
        {
            Touch(DateTimeOffset.UtcNow);
        }
    }

    /// <summary>
    /// Marks the session as started at the specified timestamp.
    /// </summary>
    /// <param name="timestampUtc">The timestamp in UTC when the session starts.</param>
    public void Start(DateTimeOffset timestampUtc)
    {
        if (IsStarted)
        {
            throw new InvalidOperationException("The session has already started.");
        }

        if (_participants.Count == 0)
        {
            throw new InvalidOperationException("Cannot start a session without participants.");
        }

        StartedAtUtc = EnsureUtc(timestampUtc);
        Touch(StartedAtUtc.Value);
    }

    /// <summary>
    /// Marks the session as completed at the specified timestamp.
    /// </summary>
    /// <param name="timestampUtc">The timestamp in UTC when the session completes.</param>
    public void Complete(DateTimeOffset timestampUtc)
    {
        if (!IsStarted)
        {
            throw new InvalidOperationException("The session must start before it can complete.");
        }

        if (IsCompleted)
        {
            throw new InvalidOperationException("The session is already completed.");
        }

        DateTimeOffset normalized = EnsureUtc(timestampUtc);

        if (normalized < StartedAtUtc)
        {
            throw new InvalidOperationException("Completion timestamp cannot precede the start timestamp.");
        }

        CompletedAtUtc = normalized;
        Touch(normalized);
    }

    /// <summary>
    /// Registers a round log and updates the session timeline.
    /// </summary>
    /// <param name="log">The round log to register.</param>
    public void RegisterRound(RoundLog log)
    {
        if (log is null)
        {
            throw new ArgumentNullException(nameof(log));
        }

        if (log.GameId != Id)
        {
            throw new InvalidOperationException("The round log does not belong to this session.");
        }

        if (_roundLogs.Count >= MaxRounds)
        {
            throw new InvalidOperationException("The session already reached its configured number of rounds.");
        }

        _roundLogs.Add(log);
        Touch(log.LoggedAtUtc);
    }

    private void Touch(DateTimeOffset timestampUtc)
    {
        DateTimeOffset normalized = EnsureUtc(timestampUtc);
        if (normalized > LastUpdatedAtUtc)
        {
            LastUpdatedAtUtc = normalized;
        }
    }

    private static DateTimeOffset EnsureUtc(DateTimeOffset timestamp)
    {
        return timestamp.Offset == TimeSpan.Zero
            ? timestamp
            : timestamp.ToUniversalTime();
    }
}
