using Microsoft.Data.Sqlite;
using SQLitePCL;
using System.Diagnostics;

namespace EmojitClient.Maui.Data;

/// <summary>
/// Provides platform-specific access to file paths and resource streams.
/// </summary>
public interface IFileProviderService
{
    /// <summary>
    /// Returns the full path where the database should be stored locally.
    /// </summary>
    string GetAppDataPath(string fileName);

    /// <summary>
    /// Opens a stream from an embedded or packaged resource.
    /// </summary>
    Task<Stream> OpenPackageFileAsync(string fileName);
}

/// <summary>
/// Manages SQLite database lifecycle (copy from assets, validation, version check).
/// </summary>
public class DatabaseService
{
    private readonly string dbPath;
    private readonly IFileProviderService fileProvider;
    private const string DbFileName = "emojit.db";
    private static bool sqliteInitialized = false;

    public DatabaseService(IFileProviderService fileProvider)
    {
        if (!sqliteInitialized)
        {
            Batteries.Init();
            sqliteInitialized = true;
        }

        this.fileProvider = fileProvider;
        dbPath = fileProvider.GetAppDataPath(DbFileName);
    }

    private static bool initializing = false;

    public async Task EnsureDatabaseReadyAsync()
    {
        if (initializing) return;
        initializing = true;

        try
        {
            Debug.WriteLine($"[DB] Checking database at: {dbPath}");

            if (!File.Exists(dbPath))
                await CopyPreloadedDatabaseAsync();

            using SqliteConnection connection = new($"Data Source={dbPath}");
            await connection.OpenAsync();
            Debug.WriteLine("[DB] Database successfully opened.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DB] Corrupted DB, recreating... ({ex.Message})");
            await CopyPreloadedDatabaseAsync(overwrite: true);
        }
        finally
        {
            initializing = false;
        }
    }

    private async Task CopyPreloadedDatabaseAsync(bool overwrite = false)
    {
        if (File.Exists(dbPath) && !overwrite)
            return;

        Debug.WriteLine($"[DB] Copying database from package (overwrite={overwrite})...");

        using Stream input = await fileProvider.OpenPackageFileAsync(DbFileName);
        using FileStream output = File.Create(dbPath);
        await input.CopyToAsync(output);

        Debug.WriteLine($"[DB] Database copied to: {dbPath}");
    }

    public SqliteConnection GetConnection()
    {
        return new SqliteConnection($"Data Source={dbPath}");
    }
}