using EmojitClient.Maui.Abstractions.Authentication;
using EmojitClient.Maui.Exceptions;
using EmojitClient.Maui.Models.Authentication;
using EmojitClient.Maui.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmojitClient.Maui.Services.Authentication;

/// <summary>
/// Provides an <see cref="IAuthenticationApiClient"/> implementation backed by <see cref="HttpClient"/>.
/// </summary>
public sealed class AuthenticationApiClient : IAuthenticationApiClient
{
    private readonly HttpClient _httpClient;
    private readonly EmojitApiOptions _options;
    private readonly ILogger<AuthenticationApiClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationApiClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client configured for the Emojit API.</param>
    /// <param name="optionsAccessor">Provides access to the configured API options.</param>
    /// <param name="logger">The logger instance.</param>
    public AuthenticationApiClient(
        HttpClient httpClient,
        IOptions<EmojitApiOptions> optionsAccessor,
        ILogger<AuthenticationApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<TokenResponse> RequestTokenAsync(TokenRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            using HttpResponseMessage response = await _httpClient
                .PostAsJsonAsync(_options.AuthenticationEndpoint, request, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogWarning(
                    "Failed to request authentication token. StatusCode={StatusCode}, Payload={Payload}.",
                    response.StatusCode,
                    responseBody);

                string message = string.IsNullOrWhiteSpace(responseBody)
                    ? $"Authentication failed with status code {(int)response.StatusCode}."
                    : responseBody.Trim();

                throw new EmojitApiException(message);
            }

            TokenResponse? token = await response.Content
                .ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
            {
                throw new EmojitApiException("The authentication endpoint returned an empty payload.");
            }

            return token;
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
            _logger.LogError(ex, "HTTP error while requesting an authentication token.");
            throw new EmojitApiException("Unable to reach the authentication endpoint.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while requesting an authentication token.");
            throw new EmojitApiException("An unexpected error occurred while requesting an authentication token.", ex);
        }
    }
}
