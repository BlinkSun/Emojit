using EmojitServer.Domain.ValueObjects;

namespace EmojitServer.Domain.Entities;

/// <summary>
/// Represents a player registered on the Emojit platform.
/// </summary>
public sealed class Player
{
    private const int MaxDisplayNameLength = 32;

    private Player()
    {
        DisplayName = string.Empty;
    }

    private Player(PlayerId id, string displayName, DateTimeOffset createdAtUtc)
    {
        Id = id;
        DisplayName = displayName;
        CreatedAtUtc = createdAtUtc;
        LastActiveAtUtc = createdAtUtc;
    }

    /// <summary>
    /// Gets the unique identifier of the player.
    /// </summary>
    public PlayerId Id { get; private set; }

    /// <summary>
    /// Gets the display name chosen by the player.
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Gets the timestamp when the player was created in UTC.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; private set; }

    /// <summary>
    /// Gets the timestamp of the most recent activity observed for the player in UTC.
    /// </summary>
    public DateTimeOffset LastActiveAtUtc { get; private set; }

    /// <summary>
    /// Gets the total number of games played by the player.
    /// </summary>
    public int GamesPlayed { get; private set; }

    /// <summary>
    /// Gets the total number of games won by the player.
    /// </summary>
    public int GamesWon { get; private set; }

    /// <summary>
    /// Creates a new <see cref="Player"/> instance with validated invariants.
    /// </summary>
    /// <param name="id">The unique identifier of the player.</param>
    /// <param name="displayName">The human-readable display name of the player.</param>
    /// <param name="createdAtUtc">The creation timestamp in UTC.</param>
    /// <returns>A fully initialized <see cref="Player"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided arguments violate invariants.</exception>
    public static Player Create(PlayerId id, string displayName, DateTimeOffset createdAtUtc)
    {
        if (id.IsEmpty)
        {
            throw new ArgumentException("Player identifier must be defined.", nameof(id));
        }

        string normalizedDisplayName = NormalizeDisplayName(displayName);

        return new Player(id, normalizedDisplayName, EnsureUtc(createdAtUtc));
    }

    /// <summary>
    /// Updates the player's display name while enforcing invariants.
    /// </summary>
    /// <param name="newDisplayName">The new display name to assign.</param>
    public void UpdateDisplayName(string newDisplayName)
    {
        string normalizedDisplayName = NormalizeDisplayName(newDisplayName);
        DisplayName = normalizedDisplayName;
    }

    /// <summary>
    /// Registers the result of a completed game for the player.
    /// </summary>
    /// <param name="won">Indicates whether the player won the game.</param>
    /// <param name="timestampUtc">The timestamp for when the game finished in UTC.</param>
    public void RegisterGameResult(bool won, DateTimeOffset timestampUtc)
    {
        DateTimeOffset normalizedTimestamp = EnsureUtc(timestampUtc);

        GamesPlayed++;
        if (won)
        {
            GamesWon++;
        }

        LastActiveAtUtc = normalizedTimestamp > LastActiveAtUtc ? normalizedTimestamp : LastActiveAtUtc;
    }

    /// <summary>
    /// Refreshes the last activity timestamp to the provided UTC value if it is newer.
    /// </summary>
    /// <param name="timestampUtc">The UTC timestamp marking the latest activity.</param>
    public void Touch(DateTimeOffset timestampUtc)
    {
        DateTimeOffset normalizedTimestamp = EnsureUtc(timestampUtc);

        if (normalizedTimestamp > LastActiveAtUtc)
        {
            LastActiveAtUtc = normalizedTimestamp;
        }
    }

    private static string NormalizeDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name is required.", nameof(displayName));
        }

        string trimmed = displayName.Trim();

        if (trimmed.Length > MaxDisplayNameLength)
        {
            throw new ArgumentException($"Display name cannot exceed {MaxDisplayNameLength} characters.", nameof(displayName));
        }

        return trimmed;
    }

    private static DateTimeOffset EnsureUtc(DateTimeOffset timestamp)
    {
        return timestamp.Offset == TimeSpan.Zero
            ? timestamp
            : timestamp.ToUniversalTime();
    }
}
