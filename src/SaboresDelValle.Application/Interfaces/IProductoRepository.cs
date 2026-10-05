using SaboresDelValle.Domain.Models;

namespace SaboresDelValle.Application.Interfaces;

public interface IProductoRepository : IRepository<Producto>
{
    Task<bool> ExisteNombreAsync(string nombre, int? idExcluido);
}
