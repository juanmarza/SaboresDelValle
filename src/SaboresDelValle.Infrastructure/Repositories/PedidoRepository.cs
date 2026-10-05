using MySqlConnector;
using SaboresDelValle.Application.Interfaces;
using SaboresDelValle.Domain.Models;
using SaboresDelValle.Infrastructure.Persistence;

namespace SaboresDelValle.Infrastructure.Repositories;

public class PedidoRepository : IPedidoRepository
{
    private const string Entidad = "pedido";

    private const string SelectPedido = """
        SELECT IdPedido, IdPlato, NumeroMesa, Cantidad, PrecioUnitario,
               Estado, MetodoPago, Propina, Descuento, FechaHora
        FROM pedido
        """;

    private readonly MySqlDataSource _dataSource;

    public PedidoRepository(MySqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<int> InsertAsync(Pedido entity)
    {
        const string sql = """
            INSERT INTO pedido (IdPlato, NumeroMesa, Cantidad, PrecioUnitario,
                                Estado, MetodoPago, Propina, Descuento, FechaHora)
            VALUES (@IdPlato, @NumeroMesa, @Cantidad, @PrecioUnitario,
                    @Estado, @MetodoPago, @Propina, @Descuento, @FechaHora);
            SELECT LAST_INSERT_ID();
            """;

        await using var connection = await _dataSource.AbrirConexionAsync();
        await using var command = new MySqlCommand(sql, connection);
        AgregarParametrosDeDatos(command, entity);
        command.Parameters.AddWithValue("@FechaHora", entity.FechaHora);

        try
        {
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }
        catch (MySqlException ex) when (MySqlErrores.EsTraducible(ex))
        {
            throw MySqlErrores.Traducir(ex, Entidad);
        }
    }

    public async Task<int> UpdateAsync(Pedido entity)
    {
        const string sql = """
            UPDATE pedido
            SET IdPlato = @IdPlato,
                NumeroMesa = @NumeroMesa,
                Cantidad = @Cantidad,
                PrecioUnitario = @PrecioUnitario,
                Estado = @Estado,
                MetodoPago = @MetodoPago,
                Propina = @Propina,
                Descuento = @Descuento
            WHERE IdPedido = @IdPedido;
            """;

        await using var connection = await _dataSource.AbrirConexionAsync();
        await using var command = new MySqlCommand(sql, connection);
        AgregarParametrosDeDatos(command, entity);
        command.Parameters.AddWithValue("@IdPedido", entity.Id);

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
        const string sql = "DELETE FROM pedido WHERE IdPedido = @IdPedido;";

        await using var connection = await _dataSource.AbrirConexionAsync();
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@IdPedido", id);

        try
        {
            return await command.ExecuteNonQueryAsync();
        }
        catch (MySqlException ex) when (MySqlErrores.EsTraducible(ex))
        {
            throw MySqlErrores.Traducir(ex, Entidad);
        }
    }

    public async Task<Pedido?> SelectByIdAsync(int id)
    {
        const string sql = SelectPedido + " WHERE IdPedido = @IdPedido;";

        await using var connection = await _dataSource.AbrirConexionAsync();
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@IdPedido", id);

        await using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? Mapear(reader) : null;
    }

    public async Task<List<Pedido>> SelectAllAsync()
    {
        const string sql = SelectPedido + " ORDER BY FechaHora DESC, IdPedido DESC;";

        await using var connection = await _dataSource.AbrirConexionAsync();
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var pedidos = new List<Pedido>();

        while (await reader.ReadAsync())
        {
            pedidos.Add(Mapear(reader));
        }

        return pedidos;
    }

    private static void AgregarParametrosDeDatos(MySqlCommand command, Pedido pedido)
    {
        object metodoPago = pedido.MetodoPago is { } metodo
            ? CodigosSql.MetodosPago.ACodigo(metodo)
            : DBNull.Value;

        command.Parameters.AddWithValue("@IdPlato", pedido.IdPlato);
        command.Parameters.AddWithValue("@NumeroMesa", pedido.NumeroMesa);
        command.Parameters.AddWithValue("@Cantidad", pedido.Cantidad);
        command.Parameters.AddWithValue("@PrecioUnitario", pedido.PrecioUnitario);
        command.Parameters.AddWithValue("@Estado", CodigosSql.EstadosPedido.ACodigo(pedido.Estado));
        command.Parameters.AddWithValue("@MetodoPago", metodoPago);
        command.Parameters.AddWithValue("@Propina", pedido.Propina);
        command.Parameters.AddWithValue("@Descuento", pedido.Descuento);
    }

    private static Pedido Mapear(MySqlDataReader reader)
    {
        var ordinalMetodoPago = reader.GetOrdinal("MetodoPago");

        return new Pedido(
            reader.GetInt32(reader.GetOrdinal("IdPedido")),
            reader.GetInt32(reader.GetOrdinal("IdPlato")),
            reader.GetInt32(reader.GetOrdinal("NumeroMesa")),
            reader.GetInt32(reader.GetOrdinal("Cantidad")),
            reader.GetDecimal(reader.GetOrdinal("PrecioUnitario")),
            CodigosSql.EstadosPedido.AValor(reader.GetString(reader.GetOrdinal("Estado"))),
            reader.IsDBNull(ordinalMetodoPago)
                ? null
                : CodigosSql.MetodosPago.AValor(reader.GetString(ordinalMetodoPago)),
            reader.GetDecimal(reader.GetOrdinal("Propina")),
            reader.GetDecimal(reader.GetOrdinal("Descuento")),
            reader.GetDateTime(reader.GetOrdinal("FechaHora")));
    }
}
