using Microsoft.Data.Sqlite;
using EmojitClient.Maui.Data.Entities;
using System.Diagnostics;

namespace EmojitClient.Maui.Data;

/// <summary>
/// Provides a flexible and async access layer to all game data.
/// </summary>
public class GameDataRepository(DatabaseService db)
{

    // 🧩 THEMES
    public async Task<List<Theme>> GetThemesAsync()
    {
        List<Theme> list = [];

        using SqliteConnection conn = db.GetConnection();
        await conn.OpenAsync();

        SqliteCommand cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Description FROM Theme;";

        using SqliteDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Theme
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2)
            });
        }

        return list;
    }

    // 🧩 SYMBOLS
    public async Task<List<Symbol>> GetSymbolsByThemeAsync(int themeId, int? count = null)
    {
        List<Symbol> symbols = [];

        try
        {
            using SqliteConnection conn = db.GetConnection();
            await conn.OpenAsync();

            using SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Label, Emoji, ImageBlob, MimeType FROM Symbol WHERE ThemeId = $id;";
            cmd.Parameters.AddWithValue("$id", themeId);

            using SqliteDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                Symbol symbol = new()
                {
                    Id = reader.GetInt32(0),
                    ThemeId = themeId,
                    Label = reader.GetString(1),
                    Emoji = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    ImageBlob = reader.IsDBNull(3) ? Array.Empty<byte>() : (byte[])reader["ImageBlob"],
                    MimeType = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
                };

                symbols.Add(symbol);
            }

            // ✅ Si un nombre est demandé, on mélange et on prend ce nombre d’éléments
            if (count.HasValue && count.Value > 0 && count.Value < symbols.Count)
            {
                // Mélange aléatoire sans doublons
                symbols = [.. symbols.OrderBy(_ => Guid.NewGuid()).Take(count.Value)];
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DB] Error in GetSymbolsByThemeAsync: {ex.Message}");
        }

        return symbols;
    }

    public async Task AddSymbolAsync(Symbol symbol)
    {
        using SqliteConnection conn = db.GetConnection();
        await conn.OpenAsync();

        SqliteCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
                INSERT INTO Symbol (ThemeId, Label, Emoji, ImageBlob, MimeType)
                VALUES ($themeId, $label, $emoji, $blob, $mime)";
        cmd.Parameters.AddWithValue("$themeId", symbol.ThemeId);
        cmd.Parameters.AddWithValue("$label", symbol.Label);
        cmd.Parameters.AddWithValue("$emoji", symbol.Emoji ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$blob", symbol.ImageBlob ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$mime", symbol.MimeType ?? "image/png");

        await cmd.ExecuteNonQueryAsync();
    }

    // 🧩 PLAYER STATS
    public async Task<PlayerStats> GetPlayerStatsAsync()
    {
        using SqliteConnection conn = db.GetConnection();
        await conn.OpenAsync();

        SqliteCommand cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, GamesPlayed, BestTime, AverageScore, LastPlayed FROM PlayerStats LIMIT 1;";

        using SqliteDataReader reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new PlayerStats
            {
                Id = reader.GetInt32(0),
                GamesPlayed = reader.GetInt32(1),
                BestTime = reader.GetDouble(2),
                AverageScore = reader.GetDouble(3),
                LastPlayed = DateTime.Parse(reader.GetString(4))
            };
        }

        return new PlayerStats();
    }

    public async Task SavePlayerStatsAsync(PlayerStats stats)
    {
        using SqliteConnection conn = db.GetConnection();
        await conn.OpenAsync();

        SqliteCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
                INSERT OR REPLACE INTO PlayerStats (Id, GamesPlayed, BestTime, AverageScore, LastPlayed)
                VALUES ($id, $played, $best, $avg, $last)";
        cmd.Parameters.AddWithValue("$id", stats.Id);
        cmd.Parameters.AddWithValue("$played", stats.GamesPlayed);
        cmd.Parameters.AddWithValue("$best", stats.BestTime);
        cmd.Parameters.AddWithValue("$avg", stats.AverageScore);
        cmd.Parameters.AddWithValue("$last", stats.LastPlayed.ToString("O"));

        await cmd.ExecuteNonQueryAsync();
    }
}