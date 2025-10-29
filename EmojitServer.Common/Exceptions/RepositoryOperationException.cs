using System;

namespace EmojitServer.Common.Exceptions;

/// <summary>
/// Represents an infrastructure-level failure encountered while performing a repository operation.
/// </summary>
public sealed class RepositoryOperationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryOperationException"/> class.
    /// </summary>
    /// <param name="message">The contextual error message.</param>
    /// <param name="innerException">The underlying exception that caused the failure.</param>
    public RepositoryOperationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
