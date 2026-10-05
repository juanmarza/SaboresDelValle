using SaboresDelValle.Application.Interfaces;
using SaboresDelValle.Domain.Models;

namespace SaboresDelValle.Application.Tests.Fakes;

// ExisteNombreAsync ignora mayúsculas, como la intercalación utf8mb4_0900_ai_ci de MySQL.

public class ProductoRepositoryEnMemoria : RepositorioEnMemoria<Producto>, IProductoRepository
{
    public Task<bool> ExisteNombreAsync(string nombre, int? idExcluido)
    {
        return Task.FromResult(Filas.Any(p =>
            string.Equals(p.Nombre, nombre, StringComparison.OrdinalIgnoreCase) && p.Id != idExcluido));
    }

    protected override int IdDe(Producto entidad) => entidad.Id;

    protected override Producto Copiar(Producto p, int id) =>
        new(id, p.Nombre, p.Tipo, p.Unidad, p.StockMinimo, p.Activo);
}

public class PlatoRepositoryEnMemoria : RepositorioEnMemoria<Plato>, IPlatoRepository
{
    public Task<bool> ExisteNombreAsync(string nombre, int? idExcluido)
    {
        return Task.FromResult(Filas.Any(p =>
            string.Equals(p.Nombre, nombre, StringComparison.OrdinalIgnoreCase) && p.Id != idExcluido));
    }

    protected override int IdDe(Plato entidad) => entidad.Id;

    protected override Plato Copiar(Plato p, int id) =>
        new(id, p.Nombre, p.Precio, p.Tipo, p.Disponible);
}

public class PedidoRepositoryEnMemoria : RepositorioEnMemoria<Pedido>, IPedidoRepository
{
    protected override int IdDe(Pedido entidad) => entidad.Id;

    protected override Pedido Copiar(Pedido p, int id) =>
        new(id, p.IdPlato, p.NumeroMesa, p.Cantidad, p.PrecioUnitario,
            p.Estado, p.MetodoPago, p.Propina, p.Descuento, p.FechaHora);
}
