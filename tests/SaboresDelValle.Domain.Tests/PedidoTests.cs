using SaboresDelValle.Domain.Enums;
using SaboresDelValle.Domain.Exceptions;
using SaboresDelValle.Domain.Models;

namespace SaboresDelValle.Domain.Tests;

public class PedidoTests
{
    private static Pedido CrearPedido(
        int idPlato = 1,
        int numeroMesa = 3,
        int cantidad = 2,
        decimal precioUnitario = 15m,
        decimal propina = 0m,
        decimal descuento = 0m)
    {
        return new Pedido(idPlato, numeroMesa, cantidad, precioUnitario, propina, descuento);
    }

    [Fact]
    public void Constructor_DatosValidos_CreaPedidoPendienteSinMetodoPagoYConFecha()
    {
        var antes = DateTime.Now;

        var pedido = CrearPedido();

        Assert.Equal(EstadoPedido.Pendiente, pedido.Estado);
        Assert.Null(pedido.MetodoPago);
        Assert.InRange(pedido.FechaHora, antes, DateTime.Now);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_IdPlatoInvalido_LanzaDomainException(int idPlato)
    {
        Assert.Throws<DomainException>(() => CrearPedido(idPlato: idPlato));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_MesaInvalida_LanzaDomainException(int numeroMesa)
    {
        Assert.Throws<DomainException>(() => CrearPedido(numeroMesa: numeroMesa));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_CantidadInvalida_LanzaDomainException(int cantidad)
    {
        Assert.Throws<DomainException>(() => CrearPedido(cantidad: cantidad));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_PrecioUnitarioInvalido_LanzaDomainException(decimal precioUnitario)
    {
        Assert.Throws<DomainException>(() => CrearPedido(precioUnitario: precioUnitario));
    }

    [Fact]
    public void Constructor_PropinaNegativa_LanzaDomainExceptionConMensajeCorrecto()
    {
        var ex = Assert.Throws<DomainException>(() => CrearPedido(propina: -1m));

        Assert.Equal("La propina no puede ser negativa.", ex.Message);
    }

    [Fact]
    public void Constructor_DescuentoNegativo_LanzaDomainExceptionConMensajeCorrecto()
    {
        var ex = Assert.Throws<DomainException>(() => CrearPedido(descuento: -1m));

        Assert.Equal("El descuento no puede ser negativo.", ex.Message);
    }

    [Fact]
    public void Constructor_PropinaYDescuentoCero_SeAceptan()
    {
        var pedido = CrearPedido(propina: 0m, descuento: 0m);

        Assert.Equal(0m, pedido.Propina);
        Assert.Equal(0m, pedido.Descuento);
    }

    [Fact]
    public void Constructor_MontosConMasDeDosDecimales_SeRedondeanADosDecimales()
    {
        var pedido = CrearPedido(precioUnitario: 35.565m, propina: 2.555m, descuento: 1.004m);

        Assert.Equal(35.57m, pedido.PrecioUnitario);
        Assert.Equal(2.56m, pedido.Propina);
        Assert.Equal(1.00m, pedido.Descuento);
    }

    [Fact]
    public void Constructor_PrecioUnitarioQueRedondeaACero_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => CrearPedido(precioUnitario: 0.004m));
    }

    public static TheoryData<decimal> NegativosPequenos => new() { -0.004m, -0.005m };

    [Theory]
    [MemberData(nameof(NegativosPequenos))]
    public void Constructor_PropinaNegativaPequena_LanzaDomainExceptionSinRedondearACero(decimal propina)
    {
        Assert.Throws<DomainException>(() => CrearPedido(propina: propina));
    }

    [Theory]
    [MemberData(nameof(NegativosPequenos))]
    public void Constructor_DescuentoNegativoPequeno_LanzaDomainExceptionSinRedondearACero(decimal descuento)
    {
        Assert.Throws<DomainException>(() => CrearPedido(descuento: descuento));
    }

    [Theory]
    [MemberData(nameof(NegativosPequenos))]
    public void Constructor_PrecioUnitarioNegativoPequeno_LanzaDomainException(decimal precioUnitario)
    {
        Assert.Throws<DomainException>(() => CrearPedido(precioUnitario: precioUnitario));
    }

    [Fact]
    public void Constructor_PrecioUnitarioQueRedondeaAUnCentavo_SeAcepta()
    {
        Assert.Equal(0.01m, CrearPedido(precioUnitario: 0.005m).PrecioUnitario);
    }

    [Theory]
    [MemberData(nameof(NegativosPequenos))]
    public void CambiarPropinaYDescuento_NegativoPequeno_LanzaDomainExceptionYConservaValores(decimal valor)
    {
        var pedido = CrearPedido(propina: 3m, descuento: 1m);

        Assert.Throws<DomainException>(() => pedido.CambiarPropina(valor));
        Assert.Throws<DomainException>(() => pedido.CambiarDescuento(valor));
        Assert.Equal(3m, pedido.Propina);
        Assert.Equal(1m, pedido.Descuento);
    }

    [Fact]
    public void Constructor_PropinaQueRedondeaANegativo_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => CrearPedido(propina: -0.005m));
    }

    [Fact]
    public void CambiarMontos_ConMasDeDosDecimales_SeRedondean()
    {
        var pedido = CrearPedido();

        pedido.CambiarPrecioUnitario(10m / 3m);
        pedido.CambiarPropina(35.575m);
        pedido.CambiarDescuento(100m / 6m);

        Assert.Equal(3.33m, pedido.PrecioUnitario);
        Assert.Equal(35.58m, pedido.Propina);
        Assert.Equal(16.67m, pedido.Descuento);
    }

    [Fact]
    public void CambiarEstado_PagadoSinMetodoPago_LanzaDomainExceptionYConservaEstado()
    {
        var pedido = CrearPedido();

        Assert.Throws<DomainException>(() => pedido.CambiarEstado(EstadoPedido.Pagado, null));
        Assert.Equal(EstadoPedido.Pendiente, pedido.Estado);
    }

    [Theory]
    [InlineData(EstadoPedido.Pendiente)]
    [InlineData(EstadoPedido.EnPreparacion)]
    [InlineData(EstadoPedido.Listo)]
    public void CambiarEstado_NoPagadoConMetodoPago_LanzaDomainException(EstadoPedido estado)
    {
        var pedido = CrearPedido();

        Assert.Throws<DomainException>(() => pedido.CambiarEstado(estado, MetodoPago.Qr));
    }

    [Theory]
    [InlineData(MetodoPago.Efectivo)]
    [InlineData(MetodoPago.Qr)]
    public void CambiarEstado_PagadoConMetodoPago_RegistraPago(MetodoPago metodoPago)
    {
        var pedido = CrearPedido();

        pedido.CambiarEstado(EstadoPedido.Pagado, metodoPago);

        Assert.Equal(EstadoPedido.Pagado, pedido.Estado);
        Assert.Equal(metodoPago, pedido.MetodoPago);
    }

    [Theory]
    [InlineData(EstadoPedido.EnPreparacion)]
    [InlineData(EstadoPedido.Listo)]
    public void CambiarEstado_EstadoNoPagadoSinMetodo_CambiaEstado(EstadoPedido estado)
    {
        var pedido = CrearPedido();

        pedido.CambiarEstado(estado, null);

        Assert.Equal(estado, pedido.Estado);
    }

    [Fact]
    public void CambiarEstado_DePagadoAPendienteSinMetodo_QuitaElMetodoPago()
    {
        var pedido = CrearPedido();
        pedido.CambiarEstado(EstadoPedido.Pagado, MetodoPago.Efectivo);

        pedido.CambiarEstado(EstadoPedido.Pendiente, null);

        Assert.Equal(EstadoPedido.Pendiente, pedido.Estado);
        Assert.Null(pedido.MetodoPago);
    }

    [Fact]
    public void CambiarEstado_EstadoInexistente_LanzaDomainException()
    {
        var pedido = CrearPedido();

        Assert.Throws<DomainException>(() => pedido.CambiarEstado((EstadoPedido)99, null));
    }

    [Fact]
    public void CambiarEstado_MetodoPagoInexistente_LanzaDomainException()
    {
        var pedido = CrearPedido();

        Assert.Throws<DomainException>(() => pedido.CambiarEstado(EstadoPedido.Pagado, (MetodoPago)99));
    }

    [Fact]
    public void ConstructorConId_PagadoSinMetodoPago_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new Pedido(1, 1, 3, 2, 15m, EstadoPedido.Pagado, null, 0m, 0m, DateTime.Now));
    }

    [Fact]
    public void ConstructorConId_FechaHoraVacia_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new Pedido(1, 1, 3, 2, 15m, EstadoPedido.Pendiente, null, 0m, 0m, default));
    }

    public static TheoryData<decimal, decimal> MontosNoNegativosConRedondeo => new()
    {
        { 0m, 0m },
        { 0.005m, 0.01m },
        { 35.565m, 35.57m },
        { 35.575m, 35.58m }
    };

    [Theory]
    [MemberData(nameof(MontosNoNegativosConRedondeo))]
    public void Constructor_Propina_SeRedondeaConPrecisionPolicy(decimal propina, decimal esperada)
    {
        Assert.Equal(esperada, CrearPedido(propina: propina).Propina);
    }

    [Theory]
    [MemberData(nameof(MontosNoNegativosConRedondeo))]
    public void Constructor_Descuento_SeRedondeaConPrecisionPolicy(decimal descuento, decimal esperado)
    {
        Assert.Equal(esperado, CrearPedido(descuento: descuento).Descuento);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    public void Constructor_MesaValida_SeAcepta(int numeroMesa)
    {
        Assert.Equal(numeroMesa, CrearPedido(numeroMesa: numeroMesa).NumeroMesa);
    }

    [Fact]
    public void CambiarEstado_FlujoCompletoHastaPagado_TerminaPagadoConMetodo()
    {
        var pedido = CrearPedido();

        pedido.CambiarEstado(EstadoPedido.EnPreparacion, null);
        Assert.Equal(EstadoPedido.EnPreparacion, pedido.Estado);

        pedido.CambiarEstado(EstadoPedido.Listo, null);
        Assert.Equal(EstadoPedido.Listo, pedido.Estado);

        pedido.CambiarEstado(EstadoPedido.Pagado, MetodoPago.Efectivo);
        Assert.Equal(EstadoPedido.Pagado, pedido.Estado);
        Assert.Equal(MetodoPago.Efectivo, pedido.MetodoPago);
    }

    [Fact]
    public void CambiosInvalidos_LanzanDomainExceptionYConservanValores()
    {
        var pedido = CrearPedido(idPlato: 4, numeroMesa: 3, cantidad: 2);

        Assert.Throws<DomainException>(() => pedido.CambiarPlato(0));
        Assert.Throws<DomainException>(() => pedido.CambiarNumeroMesa(0));
        Assert.Throws<DomainException>(() => pedido.CambiarCantidad(-1));

        Assert.Equal(4, pedido.IdPlato);
        Assert.Equal(3, pedido.NumeroMesa);
        Assert.Equal(2, pedido.Cantidad);
    }

    [Fact]
    public void Cambios_NoModificanFechaHoraDelPedido()
    {
        var fecha = new DateTime(2026, 10, 4, 12, 30, 0);
        var pedido = new Pedido(1, 1, 3, 2, 15m, EstadoPedido.Pendiente, null, 0m, 0m, fecha);

        pedido.CambiarNumeroMesa(8);
        pedido.CambiarCantidad(5);
        pedido.CambiarEstado(EstadoPedido.Pagado, MetodoPago.Qr);

        Assert.Equal(fecha, pedido.FechaHora);
    }
}
