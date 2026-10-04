using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;

namespace SaboresDelValle.Infrastructure;

public static class DependencyInjection
{
    private const string ConnectionStringName = "SaboresDelValleDb";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"No se encontró la cadena de conexión '{ConnectionStringName}'.");
        }

        services.AddSingleton(new MySqlDataSource(connectionString));

        return services;
    }
}
