using EmojitClient.Maui.Framework.Abstractions.Leaderboard;
using EmojitClient.Maui.Framework.Exceptions;
using EmojitClient.Maui.Framework.Models.Leaderboard;
using EmojitClient.Maui.Framework.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmojitClient.Maui.Framework.Services.Leaderboard;

/// <summary>
/// Provides an <see cref="ILeaderboardApiClient"/> implementation backed by <see cref="HttpClient"/>.
/// </summary>
public sealed class LeaderboardApiClient : ILeaderboardApiClient
{
    private readonly HttpClient _httpClient;
    private readonly EmojitApiOptions _options;
    private readonly ILogger<LeaderboardApiClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LeaderboardApiClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client configured for the Emojit API.</param>
    /// <param name="optionsAccessor">Provides access to the configured API options.</param>
    /// <param name="logger">The logger instance.</param>
    public LeaderboardApiClient(
        HttpClient httpClient,
        IOptions<EmojitApiOptions> optionsAccessor,
        ILogger<LeaderboardApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LeaderboardEntry>> GetTopEntriesAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");
        }

        string requestUri = $"{_options.LeaderboardEndpoint}?count={count}";

        try
        {
            using HttpResponseMessage response = await _httpClient
                .GetAsync(requestUri, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogWarning(
                    "Failed to retrieve leaderboard entries. StatusCode={StatusCode}, Payload={Payload}.",
                    response.StatusCode,
                    responseBody);

                string message = string.IsNullOrWhiteSpace(responseBody)
                    ? $"Leaderboard request failed with status code {(int)response.StatusCode}."
                    : responseBody.Trim();

                throw new EmojitApiException(message);
            }

            List<LeaderboardEntry>? entries = await response.Content
                .ReadFromJsonAsync<List<LeaderboardEntry>>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (entries is null)
            {
                throw new EmojitApiException("The leaderboard endpoint returned an empty payload.");
            }

            return entries.AsReadOnly();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (EmojitApiException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while retrieving leaderboard entries.");
            throw new EmojitApiException("Unable to reach the leaderboard endpoint.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving leaderboard entries.");
            throw new EmojitApiException("An unexpected error occurred while retrieving leaderboard entries.", ex);
        }
    }
}
