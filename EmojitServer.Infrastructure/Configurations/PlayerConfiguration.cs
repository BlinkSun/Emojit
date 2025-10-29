using EmojitServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmojitServer.Infrastructure.Configurations;

/// <summary>
/// Configures the persistence model for <see cref="Player"/> entities.
/// </summary>
internal sealed class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    private const int DisplayNameMaxLength = 32;

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("Players");

        builder.HasKey(player => player.Id);

        builder.Property(player => player.Id)
            .HasConversion(ValueObjectConverters.PlayerIdConverter)
            .ValueGeneratedNever();

        builder.Property(player => player.DisplayName)
            .IsRequired()
            .HasMaxLength(DisplayNameMaxLength);

        builder.Property(player => player.CreatedAtUtc)
            .IsRequired();

        builder.Property(player => player.LastActiveAtUtc)
            .IsRequired();

        builder.Property(player => player.GamesPlayed)
            .HasDefaultValue(0);

        builder.Property(player => player.GamesWon)
            .HasDefaultValue(0);

        builder.HasIndex(player => player.DisplayName)
            .HasDatabaseName("IX_Players_DisplayName");
    }
}
