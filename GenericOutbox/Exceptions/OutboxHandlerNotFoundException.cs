using System.Runtime.Serialization;

namespace GenericOutbox.Exceptions;

public class OutboxHandlerNotFoundException : Exception
{
    public OutboxHandlerNotFoundException()
    {
    }

    public OutboxHandlerNotFoundException(string? message) : base(message)
    {
    }

    public OutboxHandlerNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}