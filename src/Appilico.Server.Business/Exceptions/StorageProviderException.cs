namespace Appilico.Server.Business.Exceptions;

/// <summary>Represents a safe, user-facing file storage provider failure.</summary>
public class StorageProviderException : Exception
{
    /// <summary>Initializes the exception.</summary>
    public StorageProviderException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}