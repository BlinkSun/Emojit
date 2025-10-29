namespace EmojitClient.Maui.Options;

/// <summary>
/// Represents configuration values required to communicate with the Emojit server.
/// </summary>
public sealed class EmojitApiOptions
{
    /// <summary>
    /// The configuration section name expected when binding from configuration providers.
    /// </summary>
    public const string SectionName = "Emojit:Api";

    private const string DefaultBaseAddress = "https://localhost:5001";
    private const string DefaultHubPath = "/hubs/game";
    private const string DefaultAuthenticationEndpoint = "/api/authentication/token";
    private const string DefaultLeaderboardEndpoint = "/api/leaderboard/top";
    private const string DefaultDesignStatsEndpoint = "/api/stats/design";

    /// <summary>
    /// Gets or sets the root address of the Emojit server.
    /// </summary>
    public string BaseAddress { get; set; } = DefaultBaseAddress;

    /// <summary>
    /// Gets or sets the relative path pointing to the SignalR game hub.
    /// </summary>
    public string GameHubPath { get; set; } = DefaultHubPath;

    /// <summary>
    /// Gets or sets the relative path of the authentication endpoint used to obtain JWT access tokens.
    /// </summary>
    public string AuthenticationEndpoint { get; set; } = DefaultAuthenticationEndpoint;

    /// <summary>
    /// Gets or sets the relative path used to retrieve leaderboard entries.
    /// </summary>
    public string LeaderboardEndpoint { get; set; } = DefaultLeaderboardEndpoint;

    /// <summary>
    /// Gets or sets the relative path used to retrieve deterministic design statistics.
    /// </summary>
    public string DesignStatsEndpoint { get; set; } = DefaultDesignStatsEndpoint;

    /// <summary>
    /// Validates the configured options, throwing when invalid values are detected.
    /// </summary>
    public void Validate()
    {
        if (!Uri.TryCreate(BaseAddress, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("A valid absolute base address must be configured for the Emojit API.");
        }

        if (string.IsNullOrWhiteSpace(GameHubPath) || !GameHubPath.StartsWith('/'))
        {
            throw new InvalidOperationException("The game hub path must be a non-empty relative path beginning with '/'.");
        }

        if (string.IsNullOrWhiteSpace(AuthenticationEndpoint) || !AuthenticationEndpoint.StartsWith('/'))
        {
            throw new InvalidOperationException("The authentication endpoint must be a non-empty relative path beginning with '/'.");
        }

        if (string.IsNullOrWhiteSpace(LeaderboardEndpoint) || !LeaderboardEndpoint.StartsWith('/'))
        {
            throw new InvalidOperationException("The leaderboard endpoint must be a non-empty relative path beginning with '/'.");
        }

        if (string.IsNullOrWhiteSpace(DesignStatsEndpoint) || !DesignStatsEndpoint.StartsWith('/'))
        {
            throw new InvalidOperationException("The design stats endpoint must be a non-empty relative path beginning with '/'.");
        }
    }

    /// <summary>
    /// Creates an absolute URI by combining the configured base address with the provided relative path.
    /// </summary>
    /// <param name="relativePath">The relative path to combine.</param>
    /// <returns>The resulting absolute <see cref="Uri"/>.</returns>
    public Uri BuildUri(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("A relative path must be provided.", nameof(relativePath));
        }

        if (!Uri.TryCreate(BaseAddress, UriKind.Absolute, out Uri? baseUri))
        {
            throw new InvalidOperationException("The configured base address is invalid.");
        }

        return new Uri(baseUri, relativePath);
    }
}
