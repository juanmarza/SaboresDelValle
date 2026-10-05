using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SaboresDelValle.Application.Exceptions;
using SaboresDelValle.Application.Interfaces;
using SaboresDelValle.Domain.Exceptions;

namespace SaboresDelValle.Web.Pages.Platos;

public class EditarModel : PageModel
{
    private const string MensajeNoEncontrado = "El plato solicitado no existe.";

    private readonly IPlatoService _platoService;

    public EditarModel(IPlatoService platoService)
    {
        _platoService = platoService;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public PlatoFormulario Plato { get; set; } = new();

    [BindProperty]
    public bool Disponible { get; set; }

    public bool NoEncontrado { get; private set; }

    [TempData]
    public string? MensajeExito { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var plato = await _platoService.ObtenerPorIdAsync(Id);

        if (plato is null)
        {
            return MostrarNoEncontrado();
        }

        Plato = new PlatoFormulario
        {
            Nombre = plato.Nombre,
            Precio = plato.Precio,
            Tipo = plato.Tipo
        };
        Disponible = plato.Disponible;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _platoService.EditarAsync(
                Id,
                Plato.Nombre ?? string.Empty,
                Plato.Precio!.Value,
                Plato.Tipo!.Value,
                Disponible);
        }
        catch (RegistroNoEncontradoException)
        {
            return MostrarNoEncontrado();
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

        MensajeExito = "Plato actualizado correctamente.";
        return RedirectToPage("Index");
    }

    private PageResult MostrarNoEncontrado()
    {
        NoEncontrado = true;
        ModelState.AddModelError(string.Empty, MensajeNoEncontrado);

        var resultado = Page();
        resultado.StatusCode = StatusCodes.Status404NotFound;
        return resultado;
    }
}
