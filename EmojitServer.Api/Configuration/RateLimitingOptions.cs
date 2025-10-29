using System.Threading.RateLimiting;

namespace EmojitServer.Api.Configuration;

/// <summary>
/// Represents the configuration contract controlling API rate limiting boundaries.
/// </summary>
public sealed class RateLimitingOptions
{
    /// <summary>
    /// The configuration section name used to bind <see cref="RateLimitingOptions"/>.
    /// </summary>
    public const string SectionName = "RateLimiting";

    private const int MaximumPermitLimit = 10_000;
    private const int MaximumWindowInSeconds = 86_400;

    private int _permitLimit = 120;
    private int _windowInSeconds = 60;
    private int _queueLimit;

    /// <summary>
    /// Gets or sets the number of requests permitted within the configured window.
    /// </summary>
    public int PermitLimit
    {
        get => _permitLimit;
        set => _permitLimit = value;
    }

    /// <summary>
    /// Gets or sets the window length, expressed in seconds.
    /// </summary>
    public int WindowInSeconds
    {
        get => _windowInSeconds;
        set => _windowInSeconds = value;
    }

    /// <summary>
    /// Gets or sets the number of queued requests allowed when the limiter is saturated.
    /// </summary>
    public int QueueLimit
    {
        get => _queueLimit;
        set => _queueLimit = value;
    }

    /// <summary>
    /// Gets or sets the processing order applied to queued requests.
    /// </summary>
    public QueueProcessingOrder QueueProcessingOrder { get; set; } = QueueProcessingOrder.OldestFirst;

    /// <summary>
    /// Validates the configured rate-limiting settings.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when any configuration value falls outside supported bounds.</exception>
    public void Validate()
    {
        if (PermitLimit <= 0)
        {
            throw new InvalidOperationException("The rate limiter permit limit must be greater than zero.");
        }

        if (PermitLimit > MaximumPermitLimit)
        {
            throw new InvalidOperationException($"The rate limiter permit limit must not exceed {MaximumPermitLimit:N0} requests per window.");
        }

        if (WindowInSeconds <= 0)
        {
            throw new InvalidOperationException("The rate limiter window must be greater than zero seconds.");
        }

        if (WindowInSeconds > MaximumWindowInSeconds)
        {
            throw new InvalidOperationException($"The rate limiter window must not exceed {MaximumWindowInSeconds:N0} seconds.");
        }

        if (QueueLimit < 0)
        {
            throw new InvalidOperationException("The rate limiter queue limit cannot be negative.");
        }
    }

    /// <summary>
    /// Creates a <see cref="TimeSpan"/> representing the configured window duration.
    /// </summary>
    /// <returns>A <see cref="TimeSpan"/> corresponding to the configured window length.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the window cannot be converted to a <see cref="TimeSpan"/>.</exception>
    public TimeSpan ToWindowTimeSpan()
    {
        try
        {
            return TimeSpan.FromSeconds(WindowInSeconds);
        }
        catch (Exception exception) when (exception is ArgumentOutOfRangeException or OverflowException)
        {
            throw new InvalidOperationException("The configured rate limiter window is outside representable bounds.", exception);
        }
    }
}
