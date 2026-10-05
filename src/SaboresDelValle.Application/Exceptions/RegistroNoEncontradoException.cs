namespace SaboresDelValle.Application.Exceptions;

public class RegistroNoEncontradoException : Exception
{
    public RegistroNoEncontradoException(string message)
        : base(message)
    {
    }
}
