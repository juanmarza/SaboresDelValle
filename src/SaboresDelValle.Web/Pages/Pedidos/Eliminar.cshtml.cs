using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SaboresDelValle.Application.Exceptions;
using SaboresDelValle.Application.Interfaces;
using SaboresDelValle.Domain.Models;

namespace SaboresDelValle.Web.Pages.Pedidos;

public class EliminarModel : PageModel
{
    private const string MensajeNoEncontrado = "El pedido solicitado no existe.";

    private readonly IPedidoService _pedidoService;
    private readonly IPlatoService _platoService;

    public EliminarModel(IPedidoService pedidoService, IPlatoService platoService)
    {
        _pedidoService = pedidoService;
        _platoService = platoService;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public Pedido? Pedido { get; private set; }

    public string? NombrePlato { get; private set; }

    public string? MensajeError { get; private set; }

    [TempData]
    public string? MensajeExito { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        return await CargarPedidoAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            await _pedidoService.EliminarAsync(Id);
        }
        catch (RegistroNoEncontradoException)
        {
            return await CargarPedidoAsync();
        }

        MensajeExito = "Pedido eliminado correctamente.";
        return RedirectToPage("Index");
    }

    private async Task<IActionResult> CargarPedidoAsync()
    {
        Pedido = await _pedidoService.ObtenerPorIdAsync(Id);

        if (Pedido is null)
        {
            MensajeError = MensajeNoEncontrado;

            var resultado = Page();
            resultado.StatusCode = StatusCodes.Status404NotFound;
            return resultado;
        }

        NombrePlato = (await _platoService.ObtenerPorIdAsync(Pedido.IdPlato))?.Nombre;
        return Page();
    }
}
