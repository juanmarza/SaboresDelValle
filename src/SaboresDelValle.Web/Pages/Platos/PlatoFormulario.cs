using System.ComponentModel.DataAnnotations;
using SaboresDelValle.Domain.Enums;

namespace SaboresDelValle.Web.Pages.Platos;

// Solo validaciones de presentación (campos sin seleccionar o vacíos).
// Las reglas de negocio las aplican Domain y Application.
public class PlatoFormulario
{
    [Display(Name = "Nombre")]
    public string? Nombre { get; set; }

    [Display(Name = "Precio (Bs.)")]
    [Required(ErrorMessage = "Ingrese el precio.")]
    public decimal? Precio { get; set; }

    [Display(Name = "Tipo")]
    [Required(ErrorMessage = "Seleccione el tipo de plato.")]
    public TipoPlato? Tipo { get; set; }
}
