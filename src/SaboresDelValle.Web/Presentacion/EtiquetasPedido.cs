using Microsoft.AspNetCore.Mvc.Rendering;
using SaboresDelValle.Domain.Enums;
using SaboresDelValle.Domain.Models;

namespace SaboresDelValle.Web.Presentacion;

public static class EtiquetasPedido
{
    public static string Estado(EstadoPedido estado) => estado switch
    {
        EstadoPedido.Pendiente => "Pendiente",
        EstadoPedido.EnPreparacion => "En preparación",
        EstadoPedido.Listo => "Listo",
        EstadoPedido.Pagado => "Pagado",
        _ => throw new ArgumentOutOfRangeException(nameof(estado), estado, null)
    };

    public static string MetodoPago(MetodoPago? metodoPago) => metodoPago switch
    {
        null => "—",
        Domain.Enums.MetodoPago.Efectivo => "Efectivo",
        Domain.Enums.MetodoPago.Qr => "QR",
        _ => throw new ArgumentOutOfRangeException(nameof(metodoPago), metodoPago, null)
    };

    public static string ClaseEstado(EstadoPedido estado) => estado switch
    {
        EstadoPedido.Pendiente => "sv-badge--pendiente",
        EstadoPedido.EnPreparacion => "sv-badge--proceso",
        EstadoPedido.Listo => "sv-badge--positivo",
        _ => "sv-badge--completado"
    };

    // Formato moneda de la cultura activa (es-BO): Bs. 1.250,50
    public static string Monto(decimal monto) => monto.ToString("C");

    public static string FechaHora(DateTime fechaHora) => fechaHora.ToString("dd/MM/yyyy HH:mm");

    public static IEnumerable<SelectListItem> OpcionesEstado() =>
        Enum.GetValues<EstadoPedido>().Select(e => new SelectListItem(Estado(e), e.ToString()));

    public static IEnumerable<SelectListItem> OpcionesMetodoPago() =>
        Enum.GetValues<MetodoPago>().Select(m => new SelectListItem(MetodoPago(m), m.ToString()));

    public static IEnumerable<SelectListItem> OpcionesPlato(IEnumerable<Plato> platos) =>
        platos.Select(p => new SelectListItem(
            $"{p.Nombre} — {Monto(p.Precio)}{(p.Disponible ? string.Empty : " (no disponible)")}",
            p.Id.ToString()));
}
