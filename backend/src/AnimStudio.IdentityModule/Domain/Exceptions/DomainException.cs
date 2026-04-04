namespace AnimStudio.IdentityModule.Domain.Exceptions;

/// <summary>Base class for all domain-level exceptions in the IdentityModule.</summary>
public class DomainException : Exception
{
    /// <summary>Machine-readable error code for the client to act on.</summary>
    public string ErrorCode { get; }

    public DomainException(string message, string errorCode = "DOMAIN_ERROR")
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public DomainException(string message, Exception innerException, string errorCode = "DOMAIN_ERROR")
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
