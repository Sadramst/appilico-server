namespace Appilico.Server.Business.Exceptions;

/// <summary>Represents a safe, user-facing payment provider failure.</summary>
public class PaymentProviderException : Exception
{
    /// <summary>Initializes the exception.</summary>
    public PaymentProviderException(string message, string? providerRequestId = null, Exception? innerException = null)
        : base(message, innerException)
    {
        ProviderRequestId = providerRequestId;
    }

    /// <summary>Provider request ID suitable for correlation logs.</summary>
    public string? ProviderRequestId { get; }
}