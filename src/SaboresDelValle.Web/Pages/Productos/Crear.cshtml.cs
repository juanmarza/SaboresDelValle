using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SaboresDelValle.Application.Exceptions;
using SaboresDelValle.Application.Interfaces;
using SaboresDelValle.Domain.Exceptions;

namespace SaboresDelValle.Web.Pages.Productos;

public class CrearModel : PageModel
{
    private readonly IProductoService _productoService;

    public CrearModel(IProductoService productoService)
    {
        _productoService = productoService;
    }

    [BindProperty]
    public ProductoFormulario Producto { get; set; } = new();

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
            await _productoService.CrearAsync(
                Producto.Nombre ?? string.Empty,
                Producto.Tipo!.Value,
                Producto.Unidad!.Value,
                Producto.StockMinimo!.Value);
        }
        catch (RegistroDuplicadoException)
        {
            ModelState.AddModelError(string.Empty, "Ya existe un producto con ese nombre.");
            return Page();
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }

        MensajeExito = "Producto creado correctamente.";
        return RedirectToPage("Index");
    }
}
