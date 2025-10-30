using Microsoft.Data.Sqlite;

namespace EmojitClient.Maui.Data;

/// <summary>
/// Seeds default data (themes, symbols) into the database if empty.
/// </summary>
public class DatabaseInitializer(DatabaseService db)
{
    public async Task SeedAsync()
    {
        using SqliteConnection connection = db.GetConnection();
        await connection.OpenAsync();

        SqliteCommand check = connection.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM Theme;";
        object? result = await check.ExecuteScalarAsync();
        long count = (result is not null) ? Convert.ToInt64(result) : 0;

        if (count > 0) return;

        // Insert a sample theme
        SqliteCommand insertTheme = connection.CreateCommand();
        insertTheme.CommandText = "INSERT INTO Theme (Name, Description) VALUES ('Food', 'Delicious emoji food pack');";
        await insertTheme.ExecuteNonQueryAsync();

        SqliteCommand getId = connection.CreateCommand();
        getId.CommandText = "SELECT last_insert_rowid();";
        object? idResult = await getId.ExecuteScalarAsync();
        long themeId = (idResult is not null) ? Convert.ToInt64(idResult) : 0;

        await AddSymbolAsync(connection, themeId, "Apple", "🍎");
        await AddSymbolAsync(connection, themeId, "Burger", "🍔");
        await AddSymbolAsync(connection, themeId, "Pizza", "🍕");
    }

    private static async Task AddSymbolAsync(SqliteConnection conn, long themeId, string label, string emoji)
    {
        SqliteCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
                INSERT INTO Symbol (ThemeId, Label, Emoji, MimeType)
                VALUES ($themeId, $label, $emoji, 'text/emoji')";
        cmd.Parameters.AddWithValue("$themeId", themeId);
        cmd.Parameters.AddWithValue("$label", label);
        cmd.Parameters.AddWithValue("$emoji", emoji);
        await cmd.ExecuteNonQueryAsync();
    }
}