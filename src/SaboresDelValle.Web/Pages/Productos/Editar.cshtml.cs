using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SaboresDelValle.Application.Exceptions;
using SaboresDelValle.Application.Interfaces;
using SaboresDelValle.Domain.Exceptions;

namespace SaboresDelValle.Web.Pages.Productos;

public class EditarModel : PageModel
{
    private const string MensajeNoEncontrado = "El producto solicitado no existe.";

    private readonly IProductoService _productoService;

    public EditarModel(IProductoService productoService)
    {
        _productoService = productoService;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public ProductoFormulario Producto { get; set; } = new();

    [BindProperty]
    public bool Activo { get; set; }

    public bool NoEncontrado { get; private set; }

    [TempData]
    public string? MensajeExito { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var producto = await _productoService.ObtenerPorIdAsync(Id);

        if (producto is null)
        {
            return MostrarNoEncontrado();
        }

        Producto = new ProductoFormulario
        {
            Nombre = producto.Nombre,
            Tipo = producto.Tipo,
            Unidad = producto.Unidad,
            StockMinimo = producto.StockMinimo
        };
        Activo = producto.Activo;

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
            await _productoService.EditarAsync(
                Id,
                Producto.Nombre ?? string.Empty,
                Producto.Tipo!.Value,
                Producto.Unidad!.Value,
                Producto.StockMinimo!.Value,
                Activo);
        }
        catch (RegistroNoEncontradoException)
        {
            return MostrarNoEncontrado();
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

        MensajeExito = "Producto actualizado correctamente.";
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
