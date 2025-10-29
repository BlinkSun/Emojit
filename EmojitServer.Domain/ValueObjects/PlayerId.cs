namespace EmojitServer.Domain.ValueObjects;

/// <summary>
/// Represents the immutable identifier of a player.
/// </summary>
public readonly record struct PlayerId
{
    /// <summary>
    /// Gets the underlying GUID value for the player identifier.
    /// </summary>
    public Guid Value { get; }

    private PlayerId(Guid value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a <see cref="PlayerId"/> from a provided <see cref="Guid"/> value.
    /// </summary>
    /// <param name="value">The GUID to wrap.</param>
    /// <returns>A new <see cref="PlayerId"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided value is <see cref="Guid.Empty"/>.</exception>
    public static PlayerId FromGuid(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Player identifier cannot be empty.", nameof(value));
        }

        return new PlayerId(value);
    }

    /// <summary>
    /// Generates a new <see cref="PlayerId"/> with a unique GUID value.
    /// </summary>
    /// <returns>The generated <see cref="PlayerId"/>.</returns>
    public static PlayerId New() => new(Guid.NewGuid());

    /// <summary>
    /// Indicates whether the identifier has not been assigned.
    /// </summary>
    public bool IsEmpty => Value == Guid.Empty;

    /// <inheritdoc />
    public override string ToString() => Value.ToString();

    /// <summary>
    /// Attempts to construct a <see cref="PlayerId"/> without throwing exceptions.
    /// </summary>
    /// <param name="value">The GUID value to wrap.</param>
    /// <param name="playerId">The resulting <see cref="PlayerId"/> when the method succeeds.</param>
    /// <returns><c>true</c> when the GUID is not empty; otherwise, <c>false</c>.</returns>
    public static bool TryFromGuid(Guid value, out PlayerId playerId)
    {
        if (value == Guid.Empty)
        {
            playerId = default;
            return false;
        }

        playerId = new PlayerId(value);
        return true;
    }
}
