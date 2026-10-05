using SaboresDelValle.Domain.Enums;
using SaboresDelValle.Domain.Models;

namespace SaboresDelValle.Application.Interfaces;

public interface IProductoService
{
    Task<List<Producto>> ListarAsync();
    Task<Producto?> ObtenerPorIdAsync(int id);
    Task<int> CrearAsync(string nombre, TipoProducto tipo, UnidadMedida unidad, decimal stockMinimo);
    Task EditarAsync(int id, string nombre, TipoProducto tipo, UnidadMedida unidad,
                     decimal stockMinimo, bool activo);
    Task EliminarAsync(int id);
}
