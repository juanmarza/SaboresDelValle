using SaboresDelValle.Domain.Enums;
using SaboresDelValle.Domain.Models;

namespace SaboresDelValle.Application.Interfaces;

public interface IPedidoService
{
    Task<List<Pedido>> ListarAsync();
    Task<Pedido?> ObtenerPorIdAsync(int id);
    Task<int> CrearAsync(int idPlato, int numeroMesa, int cantidad, decimal propina, decimal descuento);
    Task EditarAsync(int id, int idPlato, int numeroMesa, int cantidad, EstadoPedido estado,
                     MetodoPago? metodoPago, decimal propina, decimal descuento);
    Task EliminarAsync(int id);
}
