using SaboresDelValle.Application.Interfaces;
using SaboresDelValle.Application.Exceptions;
using SaboresDelValle.Application.Services;
using SaboresDelValle.Application.Tests.Fakes;
using SaboresDelValle.Domain.Enums;
using SaboresDelValle.Domain.Exceptions;
using SaboresDelValle.Domain.Models;

namespace SaboresDelValle.Application.Tests;

public class PedidoServiceTests
{
    private const int IdInexistente = 999;

    private readonly PedidoRepositoryEnMemoria _pedidos = new();
    private readonly PlatoRepositoryEnMemoria _platos = new();
    private readonly PedidoService _servicio;

    public PedidoServiceTests()
    {
        _servicio = new PedidoService(_pedidos, _platos);
    }

    private Plato PlatoExistente(string nombre, decimal precio, bool disponible = true)
    {
        var plato = new Plato(nombre, precio, TipoPlato.Segundo);

        if (!disponible)
        {
            plato.MarcarNoDisponible();
        }

        return _platos.Agregar(plato);
    }

    private Pedido PedidoExistente(Plato plato)
    {
        return _pedidos.Agregar(new Pedido(plato.Id, 3, 2, plato.Precio, 0m, 0m));
    }

    [Fact]
    public async Task CrearAsync_PlatoExistenteYDisponible_GuardaPedidoPendienteConPrecioDelPlato()
    {
        var plato = PlatoExistente("Pique macho", 45m);

        var id = await _servicio.CrearAsync(plato.Id, numeroMesa: 3, cantidad: 2, propina: 5m, descuento: 1m);

        var pedido = await _servicio.ObtenerPorIdAsync(id);
        Assert.NotNull(pedido);
        Assert.Equal(plato.Id, pedido.IdPlato);
        Assert.Equal(45m, pedido.PrecioUnitario);
        Assert.Equal(EstadoPedido.Pendiente, pedido.Estado);
        Assert.Null(pedido.MetodoPago);
    }

    [Fact]
    public async Task CrearAsync_PlatoConPrecioRedondeado_PedidoHeredaPrecioDeDosDecimales()
    {
        var plato = PlatoExistente("Pique macho", 35.565m);

        var id = await _servicio.CrearAsync(plato.Id, 3, 1, propina: 2.555m, descuento: 0m);

        var pedido = await _servicio.ObtenerPorIdAsync(id);
        Assert.Equal(35.57m, pedido!.PrecioUnitario);
        Assert.Equal(2.56m, pedido.Propina);
    }

    [Fact]
    public async Task CrearAsync_PlatoInexistente_LanzaIntegridadReferencialException()
    {
        var ex = await Assert.ThrowsAsync<IntegridadReferencialException>(() =>
            _servicio.CrearAsync(IdInexistente, 3, 1, 0m, 0m));

        Assert.Equal("El plato seleccionado no existe.", ex.Message);
        Assert.Equal(0, _pedidos.Cantidad);
    }

    [Fact]
    public async Task CrearAsync_PlatoNoDisponible_LanzaDomainException()
    {
        var plato = PlatoExistente("Pique macho", 45m, disponible: false);

        var ex = await Assert.ThrowsAsync<DomainException>(() => _servicio.CrearAsync(plato.Id, 3, 1, 0m, 0m));

        Assert.Equal("El plato seleccionado no está disponible.", ex.Message);
        Assert.Equal(0, _pedidos.Cantidad);
    }

    [Fact]
    public async Task CrearAsync_DatosInvalidos_LanzaDomainExceptionYNoGuarda()
    {
        var plato = PlatoExistente("Pique macho", 45m);

        await Assert.ThrowsAsync<DomainException>(() => _servicio.CrearAsync(plato.Id, numeroMesa: 0, 1, 0m, 0m));
        Assert.Equal(0, _pedidos.Cantidad);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_IdInexistente_DevuelveNull()
    {
        Assert.Null(await _servicio.ObtenerPorIdAsync(IdInexistente));
    }

    [Fact]
    public async Task EditarAsync_DatosValidos_ActualizaPedido()
    {
        var plato = PlatoExistente("Pique macho", 45m);
        var pedido = PedidoExistente(plato);

        await _servicio.EditarAsync(pedido.Id, plato.Id, numeroMesa: 5, cantidad: 4,
            EstadoPedido.EnPreparacion, metodoPago: null, propina: 2m, descuento: 1m);

        var editado = await _servicio.ObtenerPorIdAsync(pedido.Id);
        Assert.Equal(5, editado!.NumeroMesa);
        Assert.Equal(4, editado.Cantidad);
        Assert.Equal(EstadoPedido.EnPreparacion, editado.Estado);
        Assert.Equal(2m, editado.Propina);
        Assert.Equal(1m, editado.Descuento);
        Assert.Equal(pedido.FechaHora, editado.FechaHora);
    }

    [Fact]
    public async Task EditarAsync_CambiaDePlato_ActualizaPrecioUnitarioAlPrecioActualDelNuevoPlato()
    {
        var sopa = PlatoExistente("Sopa de maní", 15m);
        var pique = PlatoExistente("Pique macho", 45m);
        var pedido = PedidoExistente(sopa);

        await _servicio.EditarAsync(pedido.Id, pique.Id, 3, 2, EstadoPedido.Pendiente, null, 0m, 0m);

        var editado = await _servicio.ObtenerPorIdAsync(pedido.Id);
        Assert.Equal(pique.Id, editado!.IdPlato);
        Assert.Equal(45m, editado.PrecioUnitario);
    }

    [Fact]
    public async Task EditarAsync_MismoPlato_ConservaPrecioUnitarioRegistrado()
    {
        var sopa = PlatoExistente("Sopa de maní", 15m);
        var pedido = PedidoExistente(sopa);
        sopa.CambiarPrecio(20m);
        await _platos.UpdateAsync(sopa);

        await _servicio.EditarAsync(pedido.Id, sopa.Id, 3, 2, EstadoPedido.Pendiente, null, 0m, 0m);

        Assert.Equal(15m, (await _servicio.ObtenerPorIdAsync(pedido.Id))!.PrecioUnitario);
    }

    [Fact]
    public async Task EditarAsync_CambiaAPlatoNoDisponible_LanzaDomainExceptionYNoModificaPedido()
    {
        var sopa = PlatoExistente("Sopa de maní", 15m);
        var agotado = PlatoExistente("Pique macho", 45m, disponible: false);
        var pedido = PedidoExistente(sopa);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _servicio.EditarAsync(pedido.Id, agotado.Id, 3, 2, EstadoPedido.Pendiente, null, 0m, 0m));

        Assert.Equal("El plato seleccionado no está disponible.", ex.Message);
        Assert.Equal(sopa.Id, (await _servicio.ObtenerPorIdAsync(pedido.Id))!.IdPlato);
    }

    [Fact]
    public async Task EditarAsync_CambiaAPlatoInexistente_LanzaIntegridadReferencialException()
    {
        var sopa = PlatoExistente("Sopa de maní", 15m);
        var pedido = PedidoExistente(sopa);

        await Assert.ThrowsAsync<IntegridadReferencialException>(() =>
            _servicio.EditarAsync(pedido.Id, IdInexistente, 3, 2, EstadoPedido.Pendiente, null, 0m, 0m));
    }

    [Fact]
    public async Task EditarAsync_MismoPlatoQueYaNoEstaDisponible_PermiteEditarElPedido()
    {
        var sopa = PlatoExistente("Sopa de maní", 15m);
        var pedido = PedidoExistente(sopa);
        sopa.MarcarNoDisponible();
        await _platos.UpdateAsync(sopa);

        await _servicio.EditarAsync(pedido.Id, sopa.Id, 3, 2, EstadoPedido.Listo, null, 0m, 0m);

        Assert.Equal(EstadoPedido.Listo, (await _servicio.ObtenerPorIdAsync(pedido.Id))!.Estado);
    }

    [Fact]
    public async Task EditarAsync_PagadoConMetodoPago_RegistraPago()
    {
        var plato = PlatoExistente("Pique macho", 45m);
        var pedido = PedidoExistente(plato);

        await _servicio.EditarAsync(pedido.Id, plato.Id, 3, 2, EstadoPedido.Pagado, MetodoPago.Qr, 0m, 0m);

        var editado = await _servicio.ObtenerPorIdAsync(pedido.Id);
        Assert.Equal(EstadoPedido.Pagado, editado!.Estado);
        Assert.Equal(MetodoPago.Qr, editado.MetodoPago);
    }

    [Fact]
    public async Task EditarAsync_PagadoSinMetodoPago_LanzaDomainExceptionYNoModificaPedido()
    {
        var plato = PlatoExistente("Pique macho", 45m);
        var pedido = PedidoExistente(plato);

        await Assert.ThrowsAsync<DomainException>(() =>
            _servicio.EditarAsync(pedido.Id, plato.Id, numeroMesa: 9, 2, EstadoPedido.Pagado, null, 0m, 0m));

        var guardado = await _servicio.ObtenerPorIdAsync(pedido.Id);
        Assert.Equal(EstadoPedido.Pendiente, guardado!.Estado);
        Assert.Equal(3, guardado.NumeroMesa);
    }

    [Fact]
    public async Task EditarAsync_NoPagadoConMetodoPago_LanzaDomainException()
    {
        var plato = PlatoExistente("Pique macho", 45m);
        var pedido = PedidoExistente(plato);

        await Assert.ThrowsAsync<DomainException>(() =>
            _servicio.EditarAsync(pedido.Id, plato.Id, 3, 2, EstadoPedido.Listo, MetodoPago.Efectivo, 0m, 0m));
    }

    [Fact]
    public async Task EditarAsync_IdInexistente_LanzaRegistroNoEncontradoException()
    {
        var plato = PlatoExistente("Pique macho", 45m);

        await Assert.ThrowsAsync<RegistroNoEncontradoException>(() =>
            _servicio.EditarAsync(IdInexistente, plato.Id, 3, 2, EstadoPedido.Pendiente, null, 0m, 0m));
    }

    [Fact]
    public async Task EliminarAsync_IdExistente_EliminaPedido()
    {
        var pedido = PedidoExistente(PlatoExistente("Pique macho", 45m));

        await _servicio.EliminarAsync(pedido.Id);

        Assert.Null(await _servicio.ObtenerPorIdAsync(pedido.Id));
    }

    [Fact]
    public async Task EliminarAsync_IdInexistente_LanzaRegistroNoEncontradoException()
    {
        await Assert.ThrowsAsync<RegistroNoEncontradoException>(() => _servicio.EliminarAsync(IdInexistente));
    }

    [Theory]
    [InlineData(nameof(IPedidoService.CrearAsync))]
    [InlineData(nameof(IPedidoService.EditarAsync))]
    public void Contrato_NoRecibePrecioUnitarioDelUsuario(string metodo)
    {
        var parametros = typeof(IPedidoService).GetMethod(metodo)!.GetParameters();

        Assert.DoesNotContain(parametros, p => p.Name!.Contains("precio", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task EditarAsync_FlujoDeEstadosHastaPagado_GuardaCadaEstado()
    {
        var plato = PlatoExistente("Pique macho", 45m);
        var pedido = PedidoExistente(plato);

        await _servicio.EditarAsync(pedido.Id, plato.Id, 3, 2, EstadoPedido.EnPreparacion, null, 0m, 0m);
        Assert.Equal(EstadoPedido.EnPreparacion, (await _servicio.ObtenerPorIdAsync(pedido.Id))!.Estado);

        await _servicio.EditarAsync(pedido.Id, plato.Id, 3, 2, EstadoPedido.Listo, null, 0m, 0m);
        Assert.Equal(EstadoPedido.Listo, (await _servicio.ObtenerPorIdAsync(pedido.Id))!.Estado);

        await _servicio.EditarAsync(pedido.Id, plato.Id, 3, 2, EstadoPedido.Pagado, MetodoPago.Efectivo, 0m, 0m);
        var pagado = await _servicio.ObtenerPorIdAsync(pedido.Id);
        Assert.Equal(EstadoPedido.Pagado, pagado!.Estado);
        Assert.Equal(MetodoPago.Efectivo, pagado.MetodoPago);
        Assert.Equal(45m, pagado.PrecioUnitario);
    }

    [Fact]
    public async Task EditarAsync_PropinaYDescuentoConMasDeDosDecimales_SeGuardanRedondeados()
    {
        var plato = PlatoExistente("Pique macho", 45m);
        var pedido = PedidoExistente(plato);

        await _servicio.EditarAsync(pedido.Id, plato.Id, 3, 2, EstadoPedido.Pendiente, null, 35.565m, 0.005m);

        var editado = await _servicio.ObtenerPorIdAsync(pedido.Id);
        Assert.Equal(35.57m, editado!.Propina);
        Assert.Equal(0.01m, editado.Descuento);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_IdExistente_DevuelvePedido()
    {
        var pedido = PedidoExistente(PlatoExistente("Pique macho", 45m));

        var obtenido = await _servicio.ObtenerPorIdAsync(pedido.Id);

        Assert.NotNull(obtenido);
        Assert.Equal(pedido.IdPlato, obtenido.IdPlato);
    }

    [Fact]
    public async Task ListarAsync_ConPedidos_DevuelveTodos()
    {
        var plato = PlatoExistente("Pique macho", 45m);
        PedidoExistente(plato);
        PedidoExistente(plato);

        Assert.Equal(2, (await _servicio.ListarAsync()).Count);
    }
}
