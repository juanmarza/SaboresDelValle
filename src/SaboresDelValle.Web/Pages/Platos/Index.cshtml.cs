using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SaboresDelValle.Application.Interfaces;
using SaboresDelValle.Domain.Models;

namespace SaboresDelValle.Web.Pages.Platos;

public class IndexModel : PageModel
{
    private readonly IPlatoService _platoService;

    public IndexModel(IPlatoService platoService)
    {
        _platoService = platoService;
    }

    public List<Plato> Platos { get; private set; } = new();

    [TempData]
    public string? MensajeExito { get; set; }

    public async Task OnGetAsync()
    {
        Platos = await _platoService.ListarAsync();
    }
}
