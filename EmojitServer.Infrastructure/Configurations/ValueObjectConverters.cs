using EmojitServer.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace EmojitServer.Infrastructure.Configurations;

/// <summary>
/// Provides reusable Entity Framework Core converters and comparers for domain value objects.
/// </summary>
internal static class ValueObjectConverters
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Gets the converter for persisting <see cref="PlayerId"/> values as GUID columns.
    /// </summary>
    public static readonly ValueConverter<PlayerId, Guid> PlayerIdConverter =
        new(static id => id.Value, static value => PlayerId.FromGuid(value));

    /// <summary>
    /// Gets the converter for persisting nullable <see cref="PlayerId"/> values as nullable GUID columns.
    /// </summary>
    public static readonly ValueConverter<PlayerId?, Guid?> NullablePlayerIdConverter =
        new(static id => id.HasValue ? id.Value.Value : null, static value => value.HasValue ? PlayerId.FromGuid(value.Value) : null);

    /// <summary>
    /// Gets the converter for persisting <see cref="GameId"/> values as GUID columns.
    /// </summary>
    public static readonly ValueConverter<GameId, Guid> GameIdConverter =
        new(static id => id.Value, static value => GameId.FromGuid(value));

    /// <summary>
    /// Gets the converter used to serialize participant collections into JSON.
    /// </summary>
    public static readonly ValueConverter<List<PlayerId>, string> PlayerIdCollectionConverter =
        new(
            ids => ids == null ? JsonSerializer.Serialize(Array.Empty<Guid>(), SerializerOptions) : JsonSerializer.Serialize(ids.Select(id => id.Value), SerializerOptions),
            json => DeserializeParticipants(json));

    /// <summary>
    /// Gets the comparer required for EF Core change tracking when using JSON serialized collections.
    /// </summary>
    public static readonly ValueComparer<List<PlayerId>> PlayerIdCollectionComparer =
        new(
            (left, right) =>
                left == right ||
                (!(left == null) && !(right == null) && left.SequenceEqual(right)),
            ids => ids == null ? 0 : ids.Aggregate(0, (hash, id) => HashCode.Combine(hash, id)),
            ids => ids == null ? new List<PlayerId>() : ids.ToList()
        );

    private static List<PlayerId> DeserializeParticipants(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        List<Guid>? deserialized = JsonSerializer.Deserialize<List<Guid>>(json, SerializerOptions);
        if (deserialized is null)
        {
            return [];
        }

        return deserialized
            .Where(static guid => guid != Guid.Empty)
            .Select(PlayerId.FromGuid)
            .ToList();
    }
}
