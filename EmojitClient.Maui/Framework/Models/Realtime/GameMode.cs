namespace EmojitClient.Maui.Framework.Models.Realtime;

/// <summary>
/// Represents the gameplay modes supported by the Emojit server.
/// </summary>
public enum GameMode
{
    /// <summary>
    /// Traditional tower mode where players race to match the shared card.
    /// </summary>
    Tower = 0,

    /// <summary>
    /// Future well mode variant where matches accumulate in a stack.
    /// </summary>
    Well = 1,
}
