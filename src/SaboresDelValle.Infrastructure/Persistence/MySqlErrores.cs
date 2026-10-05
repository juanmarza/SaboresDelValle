using MySqlConnector;
using SaboresDelValle.Application.Exceptions;

namespace SaboresDelValle.Infrastructure.Persistence;

internal static class MySqlErrores
{
    public static bool EsTraducible(MySqlException ex)
    {
        return ex.ErrorCode is MySqlErrorCode.DuplicateKeyEntry
            or MySqlErrorCode.RowIsReferenced2
            or MySqlErrorCode.NoReferencedRow2;
    }

    public static Exception Traducir(MySqlException ex, string entidad)
    {
        return ex.ErrorCode switch
        {
            MySqlErrorCode.DuplicateKeyEntry => new RegistroDuplicadoException(
                $"Ya existe un registro de {entidad} con un valor que debe ser único.", ex),
            MySqlErrorCode.RowIsReferenced2 => new IntegridadReferencialException(
                $"No se puede eliminar el registro de {entidad} porque otros registros dependen de él.", ex),
            MySqlErrorCode.NoReferencedRow2 => new IntegridadReferencialException(
                $"El registro de {entidad} hace referencia a un registro que no existe.", ex),
            _ => throw new ArgumentException("El error de MySQL no es traducible.", nameof(ex))
        };
    }
}
