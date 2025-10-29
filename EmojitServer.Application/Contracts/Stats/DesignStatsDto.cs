namespace EmojitServer.Application.Contracts.Stats;

/// <summary>
/// Represents statistics describing a deterministic Emojit deck design.
/// </summary>
public sealed class DesignStatsDto
{
    /// <summary>
    /// Gets or sets the mathematical order of the finite projective plane used to build the deck.
    /// </summary>
    public int Order { get; init; }

    /// <summary>
    /// Gets or sets the total number of cards available in the deck.
    /// </summary>
    public int CardCount { get; init; }

    /// <summary>
    /// Gets or sets the number of distinct symbols available.
    /// </summary>
    public int SymbolCount { get; init; }

    /// <summary>
    /// Gets or sets the number of symbols printed on each card.
    /// </summary>
    public int SymbolsPerCard { get; init; }
}
