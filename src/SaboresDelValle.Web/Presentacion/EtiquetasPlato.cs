using Microsoft.AspNetCore.Mvc.Rendering;
using SaboresDelValle.Domain.Enums;

namespace SaboresDelValle.Web.Presentacion;

public static class EtiquetasPlato
{
    public static string Tipo(TipoPlato tipo) => tipo switch
    {
        TipoPlato.Sopa => "Sopa",
        TipoPlato.Segundo => "Segundo",
        TipoPlato.Extra => "Extra",
        TipoPlato.Bebida => "Bebida",
        _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null)
    };

    public static string Estado(bool disponible) => disponible ? "Disponible" : "No disponible";

    public static string ClaseEstado(bool disponible) => disponible ? "sv-badge--positivo" : "sv-badge--neutro";

    // Formato moneda de la cultura activa (es-BO): Bs. 1.250,50
    public static string Precio(decimal precio) => precio.ToString("C");

    public static IEnumerable<SelectListItem> OpcionesTipo() =>
        Enum.GetValues<TipoPlato>().Select(t => new SelectListItem(Tipo(t), t.ToString()));
}
