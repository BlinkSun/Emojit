namespace EmojitClient.Maui.Framework.Models.Stats;

/// <summary>
/// Represents deterministic deck metrics for a given Emojit design order.
/// </summary>
public sealed class DesignStats
{
    /// <summary>
    /// Gets or sets the mathematical order defining the design.
    /// </summary>
    public int Order { get; init; }
        = 7;

    /// <summary>
    /// Gets or sets the total number of cards available in the deck.
    /// </summary>
    public int CardCount { get; init; }
        = 0;

    /// <summary>
    /// Gets or sets the number of distinct symbols.
    /// </summary>
    public int SymbolCount { get; init; }
        = 0;

    /// <summary>
    /// Gets or sets the number of symbols printed on each card.
    /// </summary>
    public int SymbolsPerCard { get; init; }
        = 0;
}
