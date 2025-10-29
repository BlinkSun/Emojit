using System;
using EmojitServer.Domain.Enums;

namespace EmojitServer.Application.Configuration;

/// <summary>
/// Represents configurable defaults governing how new games are created and managed.
/// </summary>
public sealed class GameDefaultsOptions
{
    /// <summary>
    /// The configuration section name used to bind <see cref="GameDefaultsOptions"/>.
    /// </summary>
    public const string SectionName = "GameDefaults";

    /// <summary>
    /// Gets or sets the fallback mode to use when the requested mode is invalid.
    /// </summary>
    public GameMode DefaultMode { get; set; } = GameMode.Tower;

    /// <summary>
    /// Gets or sets the default number of players for new games.
    /// </summary>
    public int DefaultMaxPlayers { get; set; } = 4;

    /// <summary>
    /// Gets or sets the default maximum number of rounds when a request omits the value.
    /// </summary>
    public int DefaultMaxRounds { get; set; } = 10;

    /// <summary>
    /// Gets or sets the minimum number of players allowed in a session.
    /// </summary>
    public int MinimumPlayers { get; set; } = 2;

    /// <summary>
    /// Gets or sets the maximum number of concurrent players allowed in a session.
    /// </summary>
    public int MaximumPlayers { get; set; } = 8;

    /// <summary>
    /// Gets or sets the minimum allowable number of rounds for a session.
    /// </summary>
    public int MinimumRounds { get; set; } = 1;

    /// <summary>
    /// Gets or sets the maximum allowable number of rounds for a session.
    /// </summary>
    public int MaximumRounds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the finite projective plane order used when generating the deterministic deck.
    /// </summary>
    public int DesignOrder { get; set; } = 7;

    /// <summary>
    /// Gets or sets a value indicating whether decks should be shuffled before starting.
    /// </summary>
    public bool ShuffleDeck { get; set; } = true;

    /// <summary>
    /// Gets or sets an optional deterministic random seed applied when shuffling decks.
    /// </summary>
    public int? RandomSeed { get; set; }

    /// <summary>
    /// Validates the configured options and throws when invalid values are detected.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the configuration is invalid.</exception>
    public void Validate()
    {
        if (!Enum.IsDefined(typeof(GameMode), DefaultMode))
        {
            throw new InvalidOperationException("The configured default game mode is not supported.");
        }

        if (MinimumPlayers < 2)
        {
            throw new InvalidOperationException("The minimum number of players must be at least two.");
        }

        if (MaximumPlayers < MinimumPlayers)
        {
            throw new InvalidOperationException("The maximum number of players cannot be less than the minimum.");
        }

        if (DefaultMaxPlayers < MinimumPlayers || DefaultMaxPlayers > MaximumPlayers)
        {
            throw new InvalidOperationException("The default player count must fall within the allowed range.");
        }

        if (MinimumRounds <= 0)
        {
            throw new InvalidOperationException("The minimum number of rounds must be greater than zero.");
        }

        if (MaximumRounds < MinimumRounds)
        {
            throw new InvalidOperationException("The maximum number of rounds cannot be less than the minimum.");
        }

        if (DefaultMaxRounds < MinimumRounds || DefaultMaxRounds > MaximumRounds)
        {
            throw new InvalidOperationException("The default round count must fall within the allowed range.");
        }

        if (DesignOrder < 3)
        {
            throw new InvalidOperationException("The deterministic deck design order must be at least three.");
        }

        if (RandomSeed.HasValue && RandomSeed.Value < 0)
        {
            throw new InvalidOperationException("The deterministic random seed cannot be negative.");
        }
    }

    /// <summary>
    /// Normalizes the requested game mode, falling back to the configured default when necessary.
    /// </summary>
    /// <param name="requested">The requested game mode.</param>
    /// <returns>The requested mode when valid, otherwise the configured default.</returns>
    public GameMode NormalizeMode(GameMode requested)
    {
        return Enum.IsDefined(typeof(GameMode), requested) ? requested : DefaultMode;
    }

    /// <summary>
    /// Normalizes the requested player count according to the configured bounds and defaults.
    /// </summary>
    /// <param name="requested">The requested player count.</param>
    /// <returns>A value constrained within the allowed range.</returns>
    public int NormalizePlayerCount(int requested)
    {
        int candidate = requested <= 0 ? DefaultMaxPlayers : requested;
        return Math.Clamp(candidate, MinimumPlayers, MaximumPlayers);
    }

    /// <summary>
    /// Normalizes the requested round count according to the configured bounds and defaults.
    /// </summary>
    /// <param name="requested">The requested round count.</param>
    /// <returns>A value constrained within the allowed range.</returns>
    public int NormalizeRoundCount(int requested)
    {
        int candidate = requested <= 0 ? DefaultMaxRounds : requested;
        return Math.Clamp(candidate, MinimumRounds, MaximumRounds);
    }
}
