using Microsoft.AspNetCore.Mvc.Rendering;
using SaboresDelValle.Domain.Enums;

namespace SaboresDelValle.Web.Presentacion;

public static class EtiquetasProducto
{
    public static string Tipo(TipoProducto tipo) => tipo switch
    {
        TipoProducto.Perecedero => "Perecedero",
        TipoProducto.NoPerecedero => "No perecedero",
        _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null)
    };

    public static string Unidad(UnidadMedida unidad) => unidad switch
    {
        UnidadMedida.Kilogramo => "Kilogramo (kg)",
        UnidadMedida.Litro => "Litro (l)",
        UnidadMedida.Unidad => "Unidad",
        _ => throw new ArgumentOutOfRangeException(nameof(unidad), unidad, null)
    };

    public static string Estado(bool activo) => activo ? "Activo" : "Inactivo";

    public static string ClaseEstado(bool activo) => activo ? "sv-badge--positivo" : "sv-badge--neutro";

    public static string StockMinimo(decimal stockMinimo) => stockMinimo.ToString("#,##0.00");

    public static IEnumerable<SelectListItem> OpcionesTipo() =>
        Enum.GetValues<TipoProducto>().Select(t => new SelectListItem(Tipo(t), t.ToString()));

    public static IEnumerable<SelectListItem> OpcionesUnidad() =>
        Enum.GetValues<UnidadMedida>().Select(u => new SelectListItem(Unidad(u), u.ToString()));
}
