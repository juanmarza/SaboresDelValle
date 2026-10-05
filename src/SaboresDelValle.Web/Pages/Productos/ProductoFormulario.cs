using System.ComponentModel.DataAnnotations;
using SaboresDelValle.Domain.Enums;

namespace SaboresDelValle.Web.Pages.Productos;

// Solo validaciones de presentación (campos sin seleccionar o vacíos).
// Las reglas de negocio las aplican Domain y Application.
public class ProductoFormulario
{
    [Display(Name = "Nombre")]
    public string? Nombre { get; set; }

    [Display(Name = "Tipo")]
    [Required(ErrorMessage = "Seleccione el tipo de producto.")]
    public TipoProducto? Tipo { get; set; }

    [Display(Name = "Unidad de medida")]
    [Required(ErrorMessage = "Seleccione la unidad de medida.")]
    public UnidadMedida? Unidad { get; set; }

    [Display(Name = "Stock mínimo")]
    [Required(ErrorMessage = "Ingrese el stock mínimo.")]
    public decimal? StockMinimo { get; set; }
}
