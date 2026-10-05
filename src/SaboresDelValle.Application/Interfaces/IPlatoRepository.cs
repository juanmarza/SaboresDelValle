using SaboresDelValle.Domain.Models;

namespace SaboresDelValle.Application.Interfaces;

public interface IPlatoRepository : IRepository<Plato>
{
    Task<bool> ExisteNombreAsync(string nombre, int? idExcluido);
}
