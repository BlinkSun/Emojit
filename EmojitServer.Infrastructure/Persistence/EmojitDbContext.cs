using EmojitServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EmojitServer.Infrastructure.Persistence;

/// <summary>
/// Represents the Entity Framework Core database context for the Emojit server solution.
/// </summary>
public sealed class EmojitDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmojitDbContext"/> class.
    /// </summary>
    /// <param name="options">The configuration options for the context.</param>
    public EmojitDbContext(DbContextOptions<EmojitDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets the database set for <see cref="Player"/> entities.
    /// </summary>
    public DbSet<Player> Players => Set<Player>();

    /// <summary>
    /// Gets the database set for <see cref="GameSession"/> aggregates.
    /// </summary>
    public DbSet<GameSession> GameSessions => Set<GameSession>();

    /// <summary>
    /// Gets the database set for <see cref="RoundLog"/> entries.
    /// </summary>
    public DbSet<RoundLog> RoundLogs => Set<RoundLog>();

    /// <summary>
    /// Gets the database set for <see cref="LeaderboardEntry"/> projections.
    /// </summary>
    public DbSet<LeaderboardEntry> LeaderboardEntries => Set<LeaderboardEntry>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
