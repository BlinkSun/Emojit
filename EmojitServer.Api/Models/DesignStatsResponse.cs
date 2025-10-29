namespace EmojitServer.Api.Models;

/// <summary>
/// Represents statistics describing a deterministic Emojit deck design.
/// </summary>
/// <param name="Order">The mathematical order of the finite projective plane used to build the deck.</param>
/// <param name="CardCount">The total number of cards available in the deck.</param>
/// <param name="SymbolCount">The number of distinct symbols available.</param>
/// <param name="SymbolsPerCard">The number of symbols printed on each card.</param>
public sealed record DesignStatsResponse(int Order, int CardCount, int SymbolCount, int SymbolsPerCard);
