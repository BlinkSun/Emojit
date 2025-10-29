using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace EmojitServer.Api.Configuration;

/// <summary>
/// Represents configuration values controlling the API CORS policy.
/// </summary>
public sealed class CorsOptions
{
    /// <summary>
    /// The configuration section name used to bind <see cref="CorsOptions"/>.
    /// </summary>
    public const string SectionName = "Cors";

    /// <summary>
    /// Gets or sets the collection of origins allowed to access the API.
    /// </summary>
    public List<string> AllowedOrigins { get; set; } = new();

    /// <summary>
    /// Gets or sets the HTTP methods permitted by the CORS policy.
    /// </summary>
    public List<string> AllowedMethods { get; set; } = new()
    {
        HttpMethods.Get,
        HttpMethods.Post,
        HttpMethods.Options,
    };

    /// <summary>
    /// Gets or sets the headers permitted by the CORS policy.
    /// </summary>
    public List<string> AllowedHeaders { get; set; } = new()
    {
        "Content-Type",
        "Authorization",
        "X-Requested-With",
        "X-SignalR-User-Agent",
    };

    /// <summary>
    /// Gets or sets a value indicating whether cross-site credentials are permitted.
    /// </summary>
    public bool AllowCredentials { get; set; }

    /// <summary>
    /// Validates the configured options and throws when invalid values are detected.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the configuration is invalid.</exception>
    public void Validate()
    {
        NormalizeOrigins();
        NormalizeMethods();
        NormalizeHeaders();

        if (AllowCredentials && AllowedOrigins.Count == 0)
        {
            throw new InvalidOperationException("At least one explicit origin must be configured when allowing credentials.");
        }

        if (AllowedOrigins.Count == 0)
        {
            throw new InvalidOperationException("At least one allowed origin must be configured for the CORS policy.");
        }
    }

    private void NormalizeOrigins()
    {
        for (int index = AllowedOrigins.Count - 1; index >= 0; index--)
        {
            string origin = AllowedOrigins[index];
            if (string.IsNullOrWhiteSpace(origin))
            {
                throw new InvalidOperationException("Allowed origins cannot contain blank entries.");
            }

            if (!Uri.TryCreate(origin, UriKind.Absolute, out Uri? parsed) || parsed.Scheme is not ("http" or "https"))
            {
                throw new InvalidOperationException($"The origin '{origin}' is not a valid HTTP or HTTPS URI.");
            }

            string normalized = parsed.GetLeftPart(UriPartial.Authority).TrimEnd('/');
            AllowedOrigins[index] = normalized;
        }

        AllowedOrigins = DistinctPreservingOrder(AllowedOrigins, StringComparer.OrdinalIgnoreCase);
    }

    private void NormalizeMethods()
    {
        if (AllowedMethods.Count == 0)
        {
            throw new InvalidOperationException("At least one HTTP method must be configured for CORS.");
        }

        for (int index = AllowedMethods.Count - 1; index >= 0; index--)
        {
            string method = AllowedMethods[index];
            if (string.IsNullOrWhiteSpace(method))
            {
                throw new InvalidOperationException("Allowed HTTP methods cannot contain blank entries.");
            }

            string normalized = method.Trim().ToUpperInvariant();

            if (!HttpMethods.IsMethod(normalized))
            {
                throw new InvalidOperationException($"The HTTP method '{method}' is not valid.");
            }

            AllowedMethods[index] = normalized;
        }

        AllowedMethods = DistinctPreservingOrder(AllowedMethods, StringComparer.OrdinalIgnoreCase);
    }

    private void NormalizeHeaders()
    {
        if (AllowedHeaders.Count == 0)
        {
            throw new InvalidOperationException("At least one header must be configured for CORS.");
        }

        for (int index = AllowedHeaders.Count - 1; index >= 0; index--)
        {
            string header = AllowedHeaders[index];
            if (string.IsNullOrWhiteSpace(header))
            {
                throw new InvalidOperationException("Allowed headers cannot contain blank entries.");
            }

            string normalized = header.Trim();
            if (normalized.Equals("*", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Wildcard headers are not permitted in strict CORS mode.");
            }

            AllowedHeaders[index] = normalized;
        }

        AllowedHeaders = DistinctPreservingOrder(AllowedHeaders, StringComparer.OrdinalIgnoreCase);
    }

    private static List<string> DistinctPreservingOrder(IEnumerable<string> values, StringComparer comparer)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(comparer);

        List<string> results = new();
        HashSet<string> seen = new(comparer);

        foreach (string value in values)
        {
            if (seen.Add(value))
            {
                results.Add(value);
            }
        }

        return results;
    }
}
