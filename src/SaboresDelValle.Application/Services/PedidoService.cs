using SaboresDelValle.Application.Exceptions;
using SaboresDelValle.Application.Interfaces;
using SaboresDelValle.Domain.Enums;
using SaboresDelValle.Domain.Exceptions;
using SaboresDelValle.Domain.Models;

namespace SaboresDelValle.Application.Services;

public class PedidoService : IPedidoService
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IPlatoRepository _platoRepository;

    public PedidoService(IPedidoRepository pedidoRepository, IPlatoRepository platoRepository)
    {
        _pedidoRepository = pedidoRepository;
        _platoRepository = platoRepository;
    }

    public Task<List<Pedido>> ListarAsync()
    {
        return _pedidoRepository.SelectAllAsync();
    }

    public Task<Pedido?> ObtenerPorIdAsync(int id)
    {
        return _pedidoRepository.SelectByIdAsync(id);
    }

    public async Task<int> CrearAsync(int idPlato, int numeroMesa, int cantidad, decimal propina, decimal descuento)
    {
        var plato = await ObtenerPlatoDisponibleAsync(idPlato);

        // El precio unitario se toma del plato en el momento de registrar el pedido.
        var pedido = new Pedido(plato.Id, numeroMesa, cantidad, plato.Precio, propina, descuento);

        return await _pedidoRepository.InsertAsync(pedido);
    }

    public async Task EditarAsync(int id, int idPlato, int numeroMesa, int cantidad, EstadoPedido estado,
                                  MetodoPago? metodoPago, decimal propina, decimal descuento)
    {
        var pedido = await _pedidoRepository.SelectByIdAsync(id) ?? throw NoEncontrado(id);

        if (pedido.IdPlato != idPlato)
        {
            var plato = await ObtenerPlatoDisponibleAsync(idPlato);
            pedido.CambiarPlato(plato.Id);
            pedido.CambiarPrecioUnitario(plato.Precio);
        }

        pedido.CambiarNumeroMesa(numeroMesa);
        pedido.CambiarCantidad(cantidad);
        pedido.CambiarPropina(propina);
        pedido.CambiarDescuento(descuento);
        pedido.CambiarEstado(estado, metodoPago);

        await _pedidoRepository.UpdateAsync(pedido);
    }

    public async Task EliminarAsync(int id)
    {
        var filasAfectadas = await _pedidoRepository.DeleteAsync(id);

        if (filasAfectadas == 0)
        {
            throw NoEncontrado(id);
        }
    }

    private async Task<Plato> ObtenerPlatoDisponibleAsync(int idPlato)
    {
        var plato = await _platoRepository.SelectByIdAsync(idPlato)
            ?? throw new IntegridadReferencialException("El plato seleccionado no existe.");

        if (!plato.Disponible)
        {
            throw new DomainException("El plato seleccionado no está disponible.");
        }

        return plato;
    }

    private static RegistroNoEncontradoException NoEncontrado(int id)
    {
        return new RegistroNoEncontradoException($"No existe un pedido con Id {id}.");
    }
}
