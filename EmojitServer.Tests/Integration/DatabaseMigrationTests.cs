using System.Threading.Tasks;
using EmojitServer.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EmojitServer.Tests.Integration;

/// <summary>
/// Ensures that Entity Framework Core migrations can be applied to a transient SQLite database.
/// </summary>
public sealed class DatabaseMigrationTests
{
    /// <summary>
    /// Applies the full migration set against an in-memory SQLite database and verifies that at least one migration executed.
    /// </summary>
    [Fact]
    public async Task ApplyingMigrations_ShouldCreateSchemaSuccessfully()
    {
        await using SqliteConnection connection = new("Data Source=:memory:");
        await connection.OpenAsync().ConfigureAwait(false);

        DbContextOptions<EmojitDbContext> options = new DbContextOptionsBuilder<EmojitDbContext>()
            .UseSqlite(connection)
            .Options;

        await using EmojitDbContext context = new(options);
        await context.Database.MigrateAsync().ConfigureAwait(false);

        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync().ConfigureAwait(false);
        Assert.NotEmpty(appliedMigrations);
    }
}
