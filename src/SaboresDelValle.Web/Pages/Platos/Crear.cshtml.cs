using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SaboresDelValle.Application.Exceptions;
using SaboresDelValle.Application.Interfaces;
using SaboresDelValle.Domain.Exceptions;

namespace SaboresDelValle.Web.Pages.Platos;

public class CrearModel : PageModel
{
    private readonly IPlatoService _platoService;

    public CrearModel(IPlatoService platoService)
    {
        _platoService = platoService;
    }

    [BindProperty]
    public PlatoFormulario Plato { get; set; } = new();

    [TempData]
    public string? MensajeExito { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _platoService.CrearAsync(
                Plato.Nombre ?? string.Empty,
                Plato.Precio!.Value,
                Plato.Tipo!.Value);
        }
        catch (RegistroDuplicadoException)
        {
            ModelState.AddModelError(string.Empty, "Ya existe un plato con ese nombre.");
            return Page();
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }

        MensajeExito = "Plato creado correctamente.";
        return RedirectToPage("Index");
    }
}
