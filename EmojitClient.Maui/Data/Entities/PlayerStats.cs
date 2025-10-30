namespace EmojitClient.Maui.Data.Entities;

/// <summary>
/// Tracks player progress and statistics.
/// </summary>
public class PlayerStats
{
    public int Id { get; set; }
    public int GamesPlayed { get; set; }
    public double BestTime { get; set; }
    public double AverageScore { get; set; }
    public DateTime LastPlayed { get; set; }
}