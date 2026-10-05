using System.ComponentModel.DataAnnotations;
using SaboresDelValle.Domain.Enums;

namespace SaboresDelValle.Web.Pages.Pedidos;

// Solo validaciones de presentación (campos sin seleccionar o vacíos).
// No existe PrecioUnitario: Application lo toma siempre del plato.
public class PedidoFormulario
{
    [Display(Name = "Plato")]
    [Required(ErrorMessage = "Seleccione un plato.")]
    public int? IdPlato { get; set; }

    [Display(Name = "Número de mesa")]
    [Required(ErrorMessage = "Ingrese el número de mesa.")]
    public int? NumeroMesa { get; set; }

    [Display(Name = "Cantidad")]
    [Required(ErrorMessage = "Ingrese la cantidad.")]
    public int? Cantidad { get; set; }

    [Display(Name = "Propina (Bs.)")]
    [Required(ErrorMessage = "Ingrese la propina (0,00 si no corresponde).")]
    public decimal? Propina { get; set; } = 0m;

    [Display(Name = "Descuento (Bs.)")]
    [Required(ErrorMessage = "Ingrese el descuento (0,00 si no corresponde).")]
    public decimal? Descuento { get; set; } = 0m;
}

public class PedidoEdicionFormulario : PedidoFormulario
{
    [Display(Name = "Estado")]
    [Required(ErrorMessage = "Seleccione el estado del pedido.")]
    public EstadoPedido? Estado { get; set; }

    [Display(Name = "Método de pago")]
    public MetodoPago? MetodoPago { get; set; }
}
