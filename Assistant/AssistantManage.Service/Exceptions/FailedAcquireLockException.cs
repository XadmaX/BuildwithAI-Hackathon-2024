namespace Assistant.Service.Exceptions;

public class FailedAcquireLockException : Exception
{
    public FailedAcquireLockException(string message) : base(message)
    {
    }

    public FailedAcquireLockException(string message, Exception innerException) : base(message, innerException)
    {
    }
}