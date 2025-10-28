using System;

namespace EmojitServer.Domain.ValueObjects;

/// <summary>
/// Represents the immutable identifier of a game session.
/// </summary>
public readonly record struct GameId
{
    /// <summary>
    /// Gets the underlying GUID value for the game identifier.
    /// </summary>
    public Guid Value { get; }

    private GameId(Guid value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a <see cref="GameId"/> from an existing <see cref="Guid"/>.
    /// </summary>
    /// <param name="value">The GUID value backing the game identifier.</param>
    /// <returns>A new <see cref="GameId"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the value is <see cref="Guid.Empty"/>.</exception>
    public static GameId FromGuid(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Game identifier cannot be empty.", nameof(value));
        }

        return new GameId(value);
    }

    /// <summary>
    /// Generates a new unique <see cref="GameId"/>.
    /// </summary>
    /// <returns>The newly generated <see cref="GameId"/>.</returns>
    public static GameId New() => new(Guid.NewGuid());

    /// <summary>
    /// Indicates whether the underlying GUID is empty.
    /// </summary>
    public bool IsEmpty => Value == Guid.Empty;

    /// <inheritdoc />
    public override string ToString() => Value.ToString();

    /// <summary>
    /// Attempts to create a <see cref="GameId"/> from the provided GUID without throwing exceptions.
    /// </summary>
    /// <param name="value">The GUID value to wrap.</param>
    /// <param name="gameId">The resulting <see cref="GameId"/> when the method returns <c>true</c>.</param>
    /// <returns><c>true</c> if the GUID is not empty and the value could be wrapped; otherwise, <c>false</c>.</returns>
    public static bool TryFromGuid(Guid value, out GameId gameId)
    {
        if (value == Guid.Empty)
        {
            gameId = default;
            return false;
        }

        gameId = new GameId(value);
        return true;
    }
}
