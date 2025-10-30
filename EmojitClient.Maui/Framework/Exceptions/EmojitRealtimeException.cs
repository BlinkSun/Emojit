namespace EmojitClient.Maui.Framework.Exceptions;

/// <summary>
/// Represents errors raised while interacting with the real-time SignalR hub.
/// </summary>
public sealed class EmojitRealtimeException : EmojitClientException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmojitRealtimeException"/> class with the provided message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public EmojitRealtimeException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmojitRealtimeException"/> class with the provided message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The underlying exception.</param>
    public EmojitRealtimeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
