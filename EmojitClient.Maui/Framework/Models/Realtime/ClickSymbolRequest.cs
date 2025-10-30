namespace EmojitClient.Maui.Framework.Models.Realtime;

/// <summary>
/// Represents the payload submitted when a player selects a symbol during an active round.
/// </summary>
public sealed class ClickSymbolRequest
{
    /// <summary>
    /// Gets or sets the identifier of the game session where the attempt occurs.
    /// </summary>
    public string GameId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the player registering the attempt.
    /// </summary>
    public string PlayerId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the symbol selected by the player.
    /// </summary>
    public int SymbolId { get; set; }
        = 0;
}
