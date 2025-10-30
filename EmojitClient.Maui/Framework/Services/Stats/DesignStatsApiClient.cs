using EmojitClient.Maui.Framework.Abstractions.Stats;
using EmojitClient.Maui.Framework.Exceptions;
using EmojitClient.Maui.Framework.Models.Stats;
using EmojitClient.Maui.Framework.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmojitClient.Maui.Framework.Services.Stats;

/// <summary>
/// Provides an <see cref="IDesignStatsApiClient"/> implementation backed by <see cref="HttpClient"/>.
/// </summary>
public sealed class DesignStatsApiClient : IDesignStatsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly EmojitApiOptions _options;
    private readonly ILogger<DesignStatsApiClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DesignStatsApiClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client configured for the Emojit API.</param>
    /// <param name="optionsAccessor">Provides access to the configured API options.</param>
    /// <param name="logger">The logger instance.</param>
    public DesignStatsApiClient(
        HttpClient httpClient,
        IOptions<EmojitApiOptions> optionsAccessor,
        ILogger<DesignStatsApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<DesignStats> GetDesignStatsAsync(int order = 7, CancellationToken cancellationToken = default)
    {
        if (order <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(order), "Order must be a positive integer.");
        }

        string requestUri = $"{_options.DesignStatsEndpoint}?order={order}";

        try
        {
            using HttpResponseMessage response = await _httpClient
                .GetAsync(requestUri, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogWarning(
                    "Failed to retrieve design stats. StatusCode={StatusCode}, Payload={Payload}.",
                    response.StatusCode,
                    responseBody);

                string message = string.IsNullOrWhiteSpace(responseBody)
                    ? $"Design stats request failed with status code {(int)response.StatusCode}."
                    : responseBody.Trim();

                throw new EmojitApiException(message);
            }

            DesignStats? stats = await response.Content
                .ReadFromJsonAsync<DesignStats>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (stats is null)
            {
                throw new EmojitApiException("The design stats endpoint returned an empty payload.");
            }

            return stats;
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
            _logger.LogError(ex, "HTTP error while retrieving design stats.");
            throw new EmojitApiException("Unable to reach the design stats endpoint.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving design stats.");
            throw new EmojitApiException("An unexpected error occurred while retrieving design stats.", ex);
        }
    }
}
