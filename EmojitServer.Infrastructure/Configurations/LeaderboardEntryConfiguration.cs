using EmojitServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmojitServer.Infrastructure.Configurations;

/// <summary>
/// Configures the persistence model for <see cref="LeaderboardEntry"/> entities.
/// </summary>
internal sealed class LeaderboardEntryConfiguration : IEntityTypeConfiguration<LeaderboardEntry>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LeaderboardEntry> builder)
    {
        builder.ToTable("LeaderboardEntries");

        builder.HasKey(entry => entry.PlayerId);

        builder.Property(entry => entry.PlayerId)
            .HasConversion(ValueObjectConverters.PlayerIdConverter)
            .ValueGeneratedNever();

        builder.Property(entry => entry.TotalPoints)
            .IsRequired();

        builder.Property(entry => entry.GamesPlayed)
            .IsRequired();

        builder.Property(entry => entry.GamesWon)
            .IsRequired();

        builder.Property(entry => entry.LastUpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(entry => entry.TotalPoints)
            .HasDatabaseName("IX_LeaderboardEntries_TotalPoints");
    }
}
