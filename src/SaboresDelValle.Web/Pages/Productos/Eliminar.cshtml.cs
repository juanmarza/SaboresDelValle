using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SaboresDelValle.Application.Exceptions;
using SaboresDelValle.Application.Interfaces;
using SaboresDelValle.Domain.Models;

namespace SaboresDelValle.Web.Pages.Productos;

public class EliminarModel : PageModel
{
    private const string MensajeNoEncontrado = "El producto solicitado no existe.";

    private readonly IProductoService _productoService;

    public EliminarModel(IProductoService productoService)
    {
        _productoService = productoService;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public Producto? Producto { get; private set; }

    public string? MensajeError { get; private set; }

    [TempData]
    public string? MensajeExito { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        return await CargarProductoAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            await _productoService.EliminarAsync(Id);
        }
        catch (RegistroNoEncontradoException)
        {
            return await CargarProductoAsync();
        }
        catch (IntegridadReferencialException)
        {
            MensajeError = "No se puede eliminar el producto porque está siendo utilizado.";
            return await CargarProductoAsync();
        }

        MensajeExito = "Producto eliminado correctamente.";
        return RedirectToPage("Index");
    }

    private async Task<IActionResult> CargarProductoAsync()
    {
        Producto = await _productoService.ObtenerPorIdAsync(Id);

        if (Producto is null)
        {
            MensajeError = MensajeNoEncontrado;

            var resultado = Page();
            resultado.StatusCode = StatusCodes.Status404NotFound;
            return resultado;
        }

        return Page();
    }
}
