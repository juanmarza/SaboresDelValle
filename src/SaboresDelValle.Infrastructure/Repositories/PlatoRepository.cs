using MySqlConnector;
using SaboresDelValle.Application.Interfaces;
using SaboresDelValle.Domain.Models;
using SaboresDelValle.Infrastructure.Persistence;

namespace SaboresDelValle.Infrastructure.Repositories;

public class PlatoRepository : IPlatoRepository
{
    private const string Entidad = "plato";

    private const string SelectPlato = """
        SELECT IdPlato, Nombre, Precio, Tipo, Disponible
        FROM plato
        """;

    private readonly MySqlDataSource _dataSource;

    public PlatoRepository(MySqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<int> InsertAsync(Plato entity)
    {
        const string sql = """
            INSERT INTO plato (Nombre, Precio, Tipo, Disponible)
            VALUES (@Nombre, @Precio, @Tipo, @Disponible);
            SELECT LAST_INSERT_ID();
            """;

        await using var connection = await _dataSource.AbrirConexionAsync();
        await using var command = new MySqlCommand(sql, connection);
        AgregarParametrosDeDatos(command, entity);

        try
        {
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }
        catch (MySqlException ex) when (MySqlErrores.EsTraducible(ex))
        {
            throw MySqlErrores.Traducir(ex, Entidad);
        }
    }

    public async Task<int> UpdateAsync(Plato entity)
    {
        const string sql = """
            UPDATE plato
            SET Nombre = @Nombre,
                Precio = @Precio,
                Tipo = @Tipo,
                Disponible = @Disponible
            WHERE IdPlato = @IdPlato;
            """;

        await using var connection = await _dataSource.AbrirConexionAsync();
        await using var command = new MySqlCommand(sql, connection);
        AgregarParametrosDeDatos(command, entity);
        command.Parameters.AddWithValue("@IdPlato", entity.Id);

        try
        {
            return await command.ExecuteNonQueryAsync();
        }
        catch (MySqlException ex) when (MySqlErrores.EsTraducible(ex))
        {
            throw MySqlErrores.Traducir(ex, Entidad);
        }
    }

    public async Task<int> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM plato WHERE IdPlato = @IdPlato;";

        await using var connection = await _dataSource.AbrirConexionAsync();
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@IdPlato", id);

        try
        {
            return await command.ExecuteNonQueryAsync();
        }
        catch (MySqlException ex) when (MySqlErrores.EsTraducible(ex))
        {
            throw MySqlErrores.Traducir(ex, Entidad);
        }
    }

    public async Task<Plato?> SelectByIdAsync(int id)
    {
        const string sql = SelectPlato + " WHERE IdPlato = @IdPlato;";

        await using var connection = await _dataSource.AbrirConexionAsync();
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@IdPlato", id);

        await using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? Mapear(reader) : null;
    }

    public async Task<List<Plato>> SelectAllAsync()
    {
        const string sql = SelectPlato + " ORDER BY Nombre;";

        await using var connection = await _dataSource.AbrirConexionAsync();
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var platos = new List<Plato>();

        while (await reader.ReadAsync())
        {
            platos.Add(Mapear(reader));
        }

        return platos;
    }

    public async Task<bool> ExisteNombreAsync(string nombre, int? idExcluido)
    {
        const string sql = """
            SELECT EXISTS (
                SELECT 1
                FROM plato
                WHERE Nombre = @Nombre
                  AND (@IdExcluido IS NULL OR IdPlato <> @IdExcluido)
            );
            """;

        await using var connection = await _dataSource.AbrirConexionAsync();
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Nombre", nombre);
        command.Parameters.AddWithValue("@IdExcluido", (object?)idExcluido ?? DBNull.Value);

        return Convert.ToInt64(await command.ExecuteScalarAsync()) == 1;
    }

    private static void AgregarParametrosDeDatos(MySqlCommand command, Plato plato)
    {
        command.Parameters.AddWithValue("@Nombre", plato.Nombre);
        command.Parameters.AddWithValue("@Precio", plato.Precio);
        command.Parameters.AddWithValue("@Tipo", CodigosSql.TiposPlato.ACodigo(plato.Tipo));
        command.Parameters.AddWithValue("@Disponible", plato.Disponible);
    }

    private static Plato Mapear(MySqlDataReader reader)
    {
        return new Plato(
            reader.GetInt32(reader.GetOrdinal("IdPlato")),
            reader.GetString(reader.GetOrdinal("Nombre")),
            reader.GetDecimal(reader.GetOrdinal("Precio")),
            CodigosSql.TiposPlato.AValor(reader.GetString(reader.GetOrdinal("Tipo"))),
            reader.GetBoolean(reader.GetOrdinal("Disponible")));
    }
}
