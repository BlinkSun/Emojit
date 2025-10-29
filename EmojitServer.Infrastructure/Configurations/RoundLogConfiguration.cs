using EmojitServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmojitServer.Infrastructure.Configurations;

/// <summary>
/// Configures the persistence model for <see cref="RoundLog"/> entities.
/// </summary>
internal sealed class RoundLogConfiguration : IEntityTypeConfiguration<RoundLog>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RoundLog> builder)
    {
        builder.ToTable("RoundLogs");

        builder.HasKey(log => log.Id);

        builder.Property(log => log.Id)
            .ValueGeneratedNever();

        builder.Property(log => log.GameId)
            .HasConversion(ValueObjectConverters.GameIdConverter)
            .IsRequired();

        builder.Property(log => log.RoundNumber)
            .IsRequired();

        builder.Property(log => log.WinningPlayerId)
            .HasConversion(ValueObjectConverters.NullablePlayerIdConverter);

        builder.Property(log => log.TowerCardIndex)
            .IsRequired();

        builder.Property(log => log.WinningPlayerCardIndex);

        builder.Property(log => log.MatchingSymbolId)
            .IsRequired();

        builder.Property(log => log.LoggedAtUtc)
            .IsRequired();

        builder.Property(log => log.ResolutionTime)
            .IsRequired();

        builder.HasIndex(log => new { log.GameId, log.RoundNumber })
            .IsUnique()
            .HasDatabaseName("UX_RoundLogs_GameId_RoundNumber");
    }
}
