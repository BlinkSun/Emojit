using System;
using System.Collections.Generic;

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
    /// Gets or sets a value indicating whether cross-site credentials are permitted.
    /// </summary>
    public bool AllowCredentials { get; set; }

    /// <summary>
    /// Validates the configured options and throws when invalid values are detected.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the configuration is invalid.</exception>
    public void Validate()
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

        if (AllowCredentials && AllowedOrigins.Count == 0)
        {
            throw new InvalidOperationException("At least one explicit origin must be configured when allowing credentials.");
        }
    }
}
