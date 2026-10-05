using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SaboresDelValle.Application.Interfaces;
using SaboresDelValle.Domain.Models;

namespace SaboresDelValle.Web.Pages.Productos;

public class IndexModel : PageModel
{
    private readonly IProductoService _productoService;

    public IndexModel(IProductoService productoService)
    {
        _productoService = productoService;
    }

    public List<Producto> Productos { get; private set; } = new();

    [TempData]
    public string? MensajeExito { get; set; }

    public async Task OnGetAsync()
    {
        Productos = await _productoService.ListarAsync();
    }
}
