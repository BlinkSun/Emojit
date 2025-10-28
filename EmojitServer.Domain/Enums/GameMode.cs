namespace EmojitServer.Domain.Enums;

/// <summary>
/// Represents the different gameplay modes supported by Emojit.
/// </summary>
public enum GameMode
{
    /// <summary>
    /// Traditional tower mode where a shared card is placed in the center and players race to match it.
    /// </summary>
    Tower = 0,

    /// <summary>
    /// Future well mode where players match against an accumulating stack of symbols.
    /// </summary>
    Well = 1,
}
