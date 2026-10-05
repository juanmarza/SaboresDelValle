using SaboresDelValle.Domain.Common;
using SaboresDelValle.Domain.Enums;
using SaboresDelValle.Domain.Exceptions;

namespace SaboresDelValle.Domain.Models;

public class Pedido
{
    public int Id { get; private set; }
    public int IdPlato { get; private set; }
    public int NumeroMesa { get; private set; }
    public int Cantidad { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public EstadoPedido Estado { get; private set; }
    public MetodoPago? MetodoPago { get; private set; }
    public decimal Propina { get; private set; }
    public decimal Descuento { get; private set; }
    public DateTime FechaHora { get; private set; }

    public Pedido(int idPlato, int numeroMesa, int cantidad, decimal precioUnitario,
                  decimal propina, decimal descuento)
    {
        ValidarIdPlato(idPlato);
        ValidarNumeroMesa(numeroMesa);
        ValidarCantidad(cantidad);

        IdPlato = idPlato;
        NumeroMesa = numeroMesa;
        Cantidad = cantidad;
        PrecioUnitario = ValidarPrecioUnitario(precioUnitario);
        Propina = ValidarPropina(propina);
        Descuento = ValidarDescuento(descuento);
        Estado = EstadoPedido.Pendiente;
        MetodoPago = null;
        FechaHora = DateTime.Now;
    }

    public Pedido(int id, int idPlato, int numeroMesa, int cantidad, decimal precioUnitario,
                  EstadoPedido estado, MetodoPago? metodoPago, decimal propina, decimal descuento,
                  DateTime fechaHora)
        : this(idPlato, numeroMesa, cantidad, precioUnitario, propina, descuento)
    {
        if (id <= 0)
        {
            throw new DomainException("El identificador del pedido debe ser mayor que cero.");
        }

        if (fechaHora == default)
        {
            throw new DomainException("La fecha y hora del pedido es obligatoria.");
        }

        ValidarEstadoYMetodoPago(estado, metodoPago);

        Id = id;
        Estado = estado;
        MetodoPago = metodoPago;
        FechaHora = fechaHora;
    }

    public void CambiarPlato(int idPlato)
    {
        ValidarIdPlato(idPlato);
        IdPlato = idPlato;
    }

    public void CambiarNumeroMesa(int numeroMesa)
    {
        ValidarNumeroMesa(numeroMesa);
        NumeroMesa = numeroMesa;
    }

    public void CambiarCantidad(int cantidad)
    {
        ValidarCantidad(cantidad);
        Cantidad = cantidad;
    }

    public void CambiarPrecioUnitario(decimal precioUnitario)
    {
        PrecioUnitario = ValidarPrecioUnitario(precioUnitario);
    }

    public void CambiarPropina(decimal propina)
    {
        Propina = ValidarPropina(propina);
    }

    public void CambiarDescuento(decimal descuento)
    {
        Descuento = ValidarDescuento(descuento);
    }

    public void CambiarEstado(EstadoPedido estado, MetodoPago? metodoPago)
    {
        ValidarEstadoYMetodoPago(estado, metodoPago);
        Estado = estado;
        MetodoPago = metodoPago;
    }

    private static void ValidarIdPlato(int idPlato)
    {
        if (idPlato <= 0)
        {
            throw new DomainException("El plato del pedido es obligatorio.");
        }
    }

    private static void ValidarNumeroMesa(int numeroMesa)
    {
        if (numeroMesa <= 0)
        {
            throw new DomainException("El número de mesa debe ser mayor que cero.");
        }
    }

    private static void ValidarCantidad(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new DomainException("La cantidad debe ser mayor que cero.");
        }
    }

    private static decimal ValidarPrecioUnitario(decimal precioUnitario)
    {
        const string mensaje = "El precio unitario debe ser mayor que cero.";

        if (precioUnitario <= 0)
        {
            throw new DomainException(mensaje);
        }

        var redondeado = PrecisionPolicy.Redondear(precioUnitario);

        if (redondeado <= 0)
        {
            throw new DomainException(mensaje);
        }

        return redondeado;
    }

    private static decimal ValidarPropina(decimal propina)
    {
        if (propina < 0)
        {
            throw new DomainException("La propina no puede ser negativa.");
        }

        var redondeada = PrecisionPolicy.Redondear(propina);

        return redondeada;
    }

    private static decimal ValidarDescuento(decimal descuento)
    {
        if (descuento < 0)
        {
            throw new DomainException("El descuento no puede ser negativo.");
        }

        var redondeado = PrecisionPolicy.Redondear(descuento);

        return redondeado;
    }

    private static void ValidarEstadoYMetodoPago(EstadoPedido estado, MetodoPago? metodoPago)
    {
        if (!Enum.IsDefined(estado))
        {
            throw new DomainException("El estado del pedido no es válido.");
        }

        if (metodoPago.HasValue && !Enum.IsDefined(metodoPago.Value))
        {
            throw new DomainException("El método de pago no es válido.");
        }

        if (estado == EstadoPedido.Pagado && metodoPago is null)
        {
            throw new DomainException("El método de pago es obligatorio cuando el pedido está pagado.");
        }

        if (estado != EstadoPedido.Pagado && metodoPago is not null)
        {
            throw new DomainException("Solo se registra método de pago cuando el pedido está pagado.");
        }
    }
}
