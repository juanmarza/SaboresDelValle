using SaboresDelValle.Domain.Enums;

namespace SaboresDelValle.Infrastructure.Persistence;

internal static class CodigosSql
{
    public static readonly MapaCodigosSql<TipoProducto> TiposProducto = new(
        "producto.Tipo",
        (TipoProducto.Perecedero, "PERECEDERO"),
        (TipoProducto.NoPerecedero, "NO_PERECEDERO"));

    public static readonly MapaCodigosSql<UnidadMedida> UnidadesMedida = new(
        "producto.UnidadMedida",
        (UnidadMedida.Kilogramo, "kg"),
        (UnidadMedida.Litro, "l"),
        (UnidadMedida.Unidad, "unidad"));

    public static readonly MapaCodigosSql<TipoPlato> TiposPlato = new(
        "plato.Tipo",
        (TipoPlato.Sopa, "SOPA"),
        (TipoPlato.Segundo, "SEGUNDO"),
        (TipoPlato.Extra, "EXTRA"),
        (TipoPlato.Bebida, "BEBIDA"));

    public static readonly MapaCodigosSql<EstadoPedido> EstadosPedido = new(
        "pedido.Estado",
        (EstadoPedido.Pendiente, "PENDIENTE"),
        (EstadoPedido.EnPreparacion, "EN_PREPARACION"),
        (EstadoPedido.Listo, "LISTO"),
        (EstadoPedido.Pagado, "PAGADO"));

    public static readonly MapaCodigosSql<MetodoPago> MetodosPago = new(
        "pedido.MetodoPago",
        (MetodoPago.Efectivo, "EFECTIVO"),
        (MetodoPago.Qr, "QR"));
}
