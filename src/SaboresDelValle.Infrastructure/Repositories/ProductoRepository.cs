using MySqlConnector;
using SaboresDelValle.Application.Interfaces;
using SaboresDelValle.Domain.Models;
using SaboresDelValle.Infrastructure.Persistence;

namespace SaboresDelValle.Infrastructure.Repositories;

public class ProductoRepository : IProductoRepository
{
    private const string Entidad = "producto";

    private const string SelectProducto = """
        SELECT IdProducto, Nombre, Tipo, UnidadMedida, StockMinimo, Activo
        FROM producto
        """;

    private readonly MySqlDataSource _dataSource;

    public ProductoRepository(MySqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<int> InsertAsync(Producto entity)
    {
        const string sql = """
            INSERT INTO producto (Nombre, Tipo, UnidadMedida, StockMinimo, Activo)
            VALUES (@Nombre, @Tipo, @UnidadMedida, @StockMinimo, @Activo);
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

    public async Task<int> UpdateAsync(Producto entity)
    {
        const string sql = """
            UPDATE producto
            SET Nombre = @Nombre,
                Tipo = @Tipo,
                UnidadMedida = @UnidadMedida,
                StockMinimo = @StockMinimo,
                Activo = @Activo
            WHERE IdProducto = @IdProducto;
            """;

        await using var connection = await _dataSource.AbrirConexionAsync();
        await using var command = new MySqlCommand(sql, connection);
        AgregarParametrosDeDatos(command, entity);
        command.Parameters.AddWithValue("@IdProducto", entity.Id);

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
        const string sql = "DELETE FROM producto WHERE IdProducto = @IdProducto;";

        await using var connection = await _dataSource.AbrirConexionAsync();
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@IdProducto", id);

        try
        {
            return await command.ExecuteNonQueryAsync();
        }
        catch (MySqlException ex) when (MySqlErrores.EsTraducible(ex))
        {
            throw MySqlErrores.Traducir(ex, Entidad);
        }
    }

    public async Task<Producto?> SelectByIdAsync(int id)
    {
        const string sql = SelectProducto + " WHERE IdProducto = @IdProducto;";

        await using var connection = await _dataSource.AbrirConexionAsync();
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@IdProducto", id);

        await using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? Mapear(reader) : null;
    }

    public async Task<List<Producto>> SelectAllAsync()
    {
        const string sql = SelectProducto + " ORDER BY Nombre;";

        await using var connection = await _dataSource.AbrirConexionAsync();
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var productos = new List<Producto>();

        while (await reader.ReadAsync())
        {
            productos.Add(Mapear(reader));
        }

        return productos;
    }

    public async Task<bool> ExisteNombreAsync(string nombre, int? idExcluido)
    {
        const string sql = """
            SELECT EXISTS (
                SELECT 1
                FROM producto
                WHERE Nombre = @Nombre
                  AND (@IdExcluido IS NULL OR IdProducto <> @IdExcluido)
            );
            """;

        await using var connection = await _dataSource.AbrirConexionAsync();
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Nombre", nombre);
        command.Parameters.AddWithValue("@IdExcluido", (object?)idExcluido ?? DBNull.Value);

        return Convert.ToInt64(await command.ExecuteScalarAsync()) == 1;
    }

    private static void AgregarParametrosDeDatos(MySqlCommand command, Producto producto)
    {
        command.Parameters.AddWithValue("@Nombre", producto.Nombre);
        command.Parameters.AddWithValue("@Tipo", CodigosSql.TiposProducto.ACodigo(producto.Tipo));
        command.Parameters.AddWithValue("@UnidadMedida", CodigosSql.UnidadesMedida.ACodigo(producto.Unidad));
        command.Parameters.AddWithValue("@StockMinimo", producto.StockMinimo);
        command.Parameters.AddWithValue("@Activo", producto.Activo);
    }

    private static Producto Mapear(MySqlDataReader reader)
    {
        return new Producto(
            reader.GetInt32(reader.GetOrdinal("IdProducto")),
            reader.GetString(reader.GetOrdinal("Nombre")),
            CodigosSql.TiposProducto.AValor(reader.GetString(reader.GetOrdinal("Tipo"))),
            CodigosSql.UnidadesMedida.AValor(reader.GetString(reader.GetOrdinal("UnidadMedida"))),
            reader.GetDecimal(reader.GetOrdinal("StockMinimo")),
            reader.GetBoolean(reader.GetOrdinal("Activo")));
    }
}
