namespace GenericOutbox.Exceptions;

public class InProgressTimeoutException : Exception
{
    public InProgressTimeoutException()
    {
    }

    public InProgressTimeoutException(string? message) : base(message)
    {
    }

    public InProgressTimeoutException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}