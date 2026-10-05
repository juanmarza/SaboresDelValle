using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SaboresDelValle.Application.Exceptions;
using SaboresDelValle.Application.Interfaces;
using SaboresDelValle.Domain.Models;

namespace SaboresDelValle.Web.Pages.Platos;

public class EliminarModel : PageModel
{
    private const string MensajeNoEncontrado = "El plato solicitado no existe.";

    private readonly IPlatoService _platoService;

    public EliminarModel(IPlatoService platoService)
    {
        _platoService = platoService;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public Plato? Plato { get; private set; }

    public string? MensajeError { get; private set; }

    [TempData]
    public string? MensajeExito { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        return await CargarPlatoAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            await _platoService.EliminarAsync(Id);
        }
        catch (RegistroNoEncontradoException)
        {
            return await CargarPlatoAsync();
        }
        catch (IntegridadReferencialException)
        {
            MensajeError = "No se puede eliminar el plato porque está siendo utilizado en un pedido.";
            return await CargarPlatoAsync();
        }

        MensajeExito = "Plato eliminado correctamente.";
        return RedirectToPage("Index");
    }

    private async Task<IActionResult> CargarPlatoAsync()
    {
        Plato = await _platoService.ObtenerPorIdAsync(Id);

        if (Plato is null)
        {
            MensajeError = MensajeNoEncontrado;

            var resultado = Page();
            resultado.StatusCode = StatusCodes.Status404NotFound;
            return resultado;
        }

        return Page();
    }
}
