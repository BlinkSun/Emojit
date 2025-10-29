namespace EmojitServer.Api.Configuration;

/// <summary>
/// Represents configuration controlling SignalR message size boundaries.
/// </summary>
public sealed class SignalRMessageOptions
{
    /// <summary>
    /// The configuration section name used to bind <see cref="SignalRMessageOptions"/>.
    /// </summary>
    public const string SectionName = "SignalRLimits";

    private const long MaximumAllowedMessageSizeInBytes = 1024L * 1024L; // 1 MiB upper guardrail.

    private long _maximumReceiveMessageSizeInBytes = 32_768L;

    /// <summary>
    /// Gets or sets the maximum incoming SignalR message size, expressed in bytes.
    /// </summary>
    /// <remarks>
    /// The default value is <c>32,768</c> bytes (32 KiB), which is sufficient for gameplay payloads
    /// while preventing oversized uploads from clients.
    /// </remarks>
    public long MaximumReceiveMessageSizeInBytes
    {
        get => _maximumReceiveMessageSizeInBytes;
        set => _maximumReceiveMessageSizeInBytes = value;
    }

    /// <summary>
    /// Validates the configured message limits.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the configured limits are outside acceptable ranges.</exception>
    public void Validate()
    {
        if (MaximumReceiveMessageSizeInBytes <= 0)
        {
            throw new InvalidOperationException("The SignalR maximum receive message size must be greater than zero bytes.");
        }

        if (MaximumReceiveMessageSizeInBytes > MaximumAllowedMessageSizeInBytes)
        {
            throw new InvalidOperationException(
                $"The SignalR maximum receive message size must not exceed {MaximumAllowedMessageSizeInBytes:N0} bytes.");
        }
    }
}
