using Microsoft.Extensions.DependencyInjection;
using SaboresDelValle.Application.Interfaces;
using SaboresDelValle.Application.Services;

namespace SaboresDelValle.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductoService, ProductoService>();
        services.AddScoped<IPlatoService, PlatoService>();
        services.AddScoped<IPedidoService, PedidoService>();

        return services;
    }
}
