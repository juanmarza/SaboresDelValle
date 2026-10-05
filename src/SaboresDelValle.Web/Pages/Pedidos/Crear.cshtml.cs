using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SaboresDelValle.Application.Exceptions;
using SaboresDelValle.Application.Interfaces;
using SaboresDelValle.Domain.Exceptions;
using SaboresDelValle.Web.Presentacion;

namespace SaboresDelValle.Web.Pages.Pedidos;

public class CrearModel : PageModel
{
    private readonly IPedidoService _pedidoService;
    private readonly IPlatoService _platoService;

    public CrearModel(IPedidoService pedidoService, IPlatoService platoService)
    {
        _pedidoService = pedidoService;
        _platoService = platoService;
    }

    [BindProperty]
    public PedidoFormulario Pedido { get; set; } = new();

    public List<SelectListItem> OpcionesPlato { get; private set; } = new();

    [TempData]
    public string? MensajeExito { get; set; }

    public async Task OnGetAsync()
    {
        await CargarPlatosDisponiblesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return await MostrarFormularioAsync();
        }

        try
        {
            await _pedidoService.CrearAsync(
                Pedido.IdPlato!.Value,
                Pedido.NumeroMesa!.Value,
                Pedido.Cantidad!.Value,
                Pedido.Propina!.Value,
                Pedido.Descuento!.Value);
        }
        catch (IntegridadReferencialException)
        {
            ModelState.AddModelError(string.Empty, "El plato seleccionado no existe.");
            return await MostrarFormularioAsync();
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return await MostrarFormularioAsync();
        }

        MensajeExito = "Pedido creado correctamente.";
        return RedirectToPage("Index");
    }

    private async Task<IActionResult> MostrarFormularioAsync()
    {
        await CargarPlatosDisponiblesAsync();
        return Page();
    }

    private async Task CargarPlatosDisponiblesAsync()
    {
        var disponibles = (await _platoService.ListarAsync()).Where(p => p.Disponible);
        OpcionesPlato = EtiquetasPedido.OpcionesPlato(disponibles).ToList();
    }
}
