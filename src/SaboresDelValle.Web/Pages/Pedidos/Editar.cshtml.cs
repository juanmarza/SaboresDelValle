using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SaboresDelValle.Application.Exceptions;
using SaboresDelValle.Application.Interfaces;
using SaboresDelValle.Domain.Exceptions;
using SaboresDelValle.Domain.Models;
using SaboresDelValle.Web.Presentacion;

namespace SaboresDelValle.Web.Pages.Pedidos;

public class EditarModel : PageModel
{
    private const string MensajeNoEncontrado = "El pedido solicitado no existe.";

    private readonly IPedidoService _pedidoService;
    private readonly IPlatoService _platoService;

    public EditarModel(IPedidoService pedidoService, IPlatoService platoService)
    {
        _pedidoService = pedidoService;
        _platoService = platoService;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public PedidoEdicionFormulario Pedido { get; set; } = new();

    // Datos de solo lectura: el precio unitario y la fecha no se editan desde el formulario.
    public Pedido? PedidoRegistrado { get; private set; }

    public List<SelectListItem> OpcionesPlato { get; private set; } = new();

    public bool NoEncontrado { get; private set; }

    [TempData]
    public string? MensajeExito { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!await CargarPedidoRegistradoAsync())
        {
            return MostrarNoEncontrado();
        }

        var pedido = PedidoRegistrado!;
        Pedido = new PedidoEdicionFormulario
        {
            IdPlato = pedido.IdPlato,
            NumeroMesa = pedido.NumeroMesa,
            Cantidad = pedido.Cantidad,
            Propina = pedido.Propina,
            Descuento = pedido.Descuento,
            Estado = pedido.Estado,
            MetodoPago = pedido.MetodoPago
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return await MostrarFormularioAsync();
        }

        try
        {
            await _pedidoService.EditarAsync(
                Id,
                Pedido.IdPlato!.Value,
                Pedido.NumeroMesa!.Value,
                Pedido.Cantidad!.Value,
                Pedido.Estado!.Value,
                Pedido.MetodoPago,
                Pedido.Propina!.Value,
                Pedido.Descuento!.Value);
        }
        catch (RegistroNoEncontradoException)
        {
            return MostrarNoEncontrado();
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

        MensajeExito = "Pedido actualizado correctamente.";
        return RedirectToPage("Index");
    }

    private async Task<IActionResult> MostrarFormularioAsync()
    {
        return await CargarPedidoRegistradoAsync() ? Page() : MostrarNoEncontrado();
    }

    private async Task<bool> CargarPedidoRegistradoAsync()
    {
        PedidoRegistrado = await _pedidoService.ObtenerPorIdAsync(Id);

        if (PedidoRegistrado is null)
        {
            return false;
        }

        // Disponibles más el plato actual del pedido, que puede conservarse aunque ya no esté disponible.
        var platos = (await _platoService.ListarAsync())
            .Where(p => p.Disponible || p.Id == PedidoRegistrado.IdPlato);
        OpcionesPlato = EtiquetasPedido.OpcionesPlato(platos).ToList();

        return true;
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
