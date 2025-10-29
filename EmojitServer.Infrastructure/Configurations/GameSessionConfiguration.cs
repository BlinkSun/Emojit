using EmojitServer.Domain.Entities;
using EmojitServer.Domain.Enums;
using EmojitServer.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EmojitServer.Infrastructure.Configurations;

/// <summary>
/// Configures the persistence model for <see cref="GameSession"/> aggregates.
/// </summary>
internal sealed class GameSessionConfiguration : IEntityTypeConfiguration<GameSession>
{
    private static readonly EnumToStringConverter<GameMode> GameModeConverter = new();

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<GameSession> builder)
    {
        builder.ToTable("GameSessions");

        builder.HasKey(session => session.Id);

        builder.Property(session => session.Id)
            .HasConversion(ValueObjectConverters.GameIdConverter)
            .ValueGeneratedNever();

        builder.Property(session => session.Mode)
            .HasConversion(GameModeConverter)
            .IsRequired();

        builder.Property(session => session.MaxPlayers)
            .IsRequired();

        builder.Property(session => session.MaxRounds)
            .IsRequired();

        builder.Property(session => session.CreatedAtUtc)
            .IsRequired();

        builder.Property(session => session.LastUpdatedAtUtc)
            .IsRequired();

        builder.Property(session => session.StartedAtUtc);
        builder.Property(session => session.CompletedAtUtc);

        builder.Ignore(session => session.Participants);

        builder.Property<List<PlayerId>>("_participants")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("Participants")
            .HasConversion(ValueObjectConverters.PlayerIdCollectionConverter)
            .Metadata.SetValueComparer(ValueObjectConverters.PlayerIdCollectionComparer);

        builder.HasMany(session => session.RoundLogs)
            .WithOne()
            .HasForeignKey(log => log.GameId)
            .HasPrincipalKey(session => session.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(session => session.RoundLogs)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(session => session.CreatedAtUtc)
            .HasDatabaseName("IX_GameSessions_CreatedAtUtc");
    }
}
