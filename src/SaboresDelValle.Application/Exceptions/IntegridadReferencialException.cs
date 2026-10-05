namespace SaboresDelValle.Application.Exceptions;

public class IntegridadReferencialException : Exception
{
    public IntegridadReferencialException(string message)
        : base(message)
    {
    }

    public IntegridadReferencialException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
