using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using EmojitServer.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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
            static ids =>
            {
                IEnumerable<Guid> values = ids?.Select(id => id.Value) ?? Array.Empty<Guid>();
                return JsonSerializer.Serialize(values, SerializerOptions);
            },
            static json => DeserializeParticipants(json));

    /// <summary>
    /// Gets the comparer required for EF Core change tracking when using JSON serialized collections.
    /// </summary>
    public static readonly ValueComparer<List<PlayerId>> PlayerIdCollectionComparer =
        new(
            (left, right) =>
            {
                if (ReferenceEquals(left, right))
                {
                    return true;
                }

                if (left is null || right is null)
                {
                    return false;
                }

                return left.SequenceEqual(right);
            },
            ids =>
            {
                if (ids is null)
                {
                    return 0;
                }

                return ids.Aggregate(0, (hash, id) => HashCode.Combine(hash, id));
            },
            ids => ids is null ? new List<PlayerId>() : ids.ToList());

    private static List<PlayerId> DeserializeParticipants(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<PlayerId>();
        }

        List<Guid>? deserialized = JsonSerializer.Deserialize<List<Guid>>(json, SerializerOptions);
        if (deserialized is null)
        {
            return new List<PlayerId>();
        }

        return deserialized
            .Where(static guid => guid != Guid.Empty)
            .Select(PlayerId.FromGuid)
            .ToList();
    }
}
