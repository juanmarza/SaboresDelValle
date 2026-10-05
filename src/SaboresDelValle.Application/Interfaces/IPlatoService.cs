using SaboresDelValle.Domain.Enums;
using SaboresDelValle.Domain.Models;

namespace SaboresDelValle.Application.Interfaces;

public interface IPlatoService
{
    Task<List<Plato>> ListarAsync();
    Task<Plato?> ObtenerPorIdAsync(int id);
    Task<int> CrearAsync(string nombre, decimal precio, TipoPlato tipo);
    Task EditarAsync(int id, string nombre, decimal precio, TipoPlato tipo, bool disponible);
    Task EliminarAsync(int id);
}
