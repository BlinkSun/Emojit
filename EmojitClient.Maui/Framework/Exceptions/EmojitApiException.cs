namespace EmojitClient.Maui.Framework.Exceptions;

/// <summary>
/// Represents REST API errors returned by the Emojit server.
/// </summary>
public sealed class EmojitApiException : EmojitClientException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmojitApiException"/> class with the provided message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public EmojitApiException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmojitApiException"/> class with the provided message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The underlying exception.</param>
    public EmojitApiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
