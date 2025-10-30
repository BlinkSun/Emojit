namespace EmojitClient.Maui.Framework.Exceptions;

/// <summary>
/// Represents errors produced while communicating with the Emojit server.
/// </summary>
public class EmojitClientException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmojitClientException"/> class.
    /// </summary>
    public EmojitClientException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmojitClientException"/> class with the provided message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public EmojitClientException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmojitClientException"/> class with the provided message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The underlying exception.</param>
    public EmojitClientException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
