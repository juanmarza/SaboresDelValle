using MySqlConnector;

namespace SaboresDelValle.Infrastructure.Persistence;

internal static class MySqlDataSourceExtensions
{
    public static async Task<MySqlConnection> AbrirConexionAsync(this MySqlDataSource dataSource)
    {
        var connection = dataSource.CreateConnection();

        try
        {
            await connection.OpenAsync();
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }
}
