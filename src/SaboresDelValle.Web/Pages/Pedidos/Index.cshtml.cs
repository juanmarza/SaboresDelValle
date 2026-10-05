using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SaboresDelValle.Application.Interfaces;
using SaboresDelValle.Domain.Models;

namespace SaboresDelValle.Web.Pages.Pedidos;

public class IndexModel : PageModel
{
    private readonly IPedidoService _pedidoService;
    private readonly IPlatoService _platoService;

    public IndexModel(IPedidoService pedidoService, IPlatoService platoService)
    {
        _pedidoService = pedidoService;
        _platoService = platoService;
    }

    public List<Pedido> Pedidos { get; private set; } = new();

    public Dictionary<int, string> NombresPlato { get; private set; } = new();

    [TempData]
    public string? MensajeExito { get; set; }

    public async Task OnGetAsync()
    {
        Pedidos = await _pedidoService.ListarAsync();
        NombresPlato = (await _platoService.ListarAsync()).ToDictionary(p => p.Id, p => p.Nombre);
    }
}
