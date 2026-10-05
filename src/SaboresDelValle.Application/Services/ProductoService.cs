using SaboresDelValle.Application.Exceptions;
using SaboresDelValle.Application.Interfaces;
using SaboresDelValle.Domain.Enums;
using SaboresDelValle.Domain.Models;

namespace SaboresDelValle.Application.Services;

public class ProductoService : IProductoService
{
    private readonly IProductoRepository _productoRepository;

    public ProductoService(IProductoRepository productoRepository)
    {
        _productoRepository = productoRepository;
    }

    public Task<List<Producto>> ListarAsync()
    {
        return _productoRepository.SelectAllAsync();
    }

    public Task<Producto?> ObtenerPorIdAsync(int id)
    {
        return _productoRepository.SelectByIdAsync(id);
    }

    public async Task<int> CrearAsync(string nombre, TipoProducto tipo, UnidadMedida unidad, decimal stockMinimo)
    {
        var producto = new Producto(nombre, tipo, unidad, stockMinimo);

        await AsegurarNombreDisponibleAsync(producto.Nombre, idExcluido: null);

        return await _productoRepository.InsertAsync(producto);
    }

    public async Task EditarAsync(int id, string nombre, TipoProducto tipo, UnidadMedida unidad,
                                  decimal stockMinimo, bool activo)
    {
        // Se reconstruye completo para validar unidad y stock mínimo en conjunto.
        var producto = new Producto(id, nombre, tipo, unidad, stockMinimo, activo);

        await AsegurarExisteAsync(id);
        await AsegurarNombreDisponibleAsync(producto.Nombre, idExcluido: id);

        await _productoRepository.UpdateAsync(producto);
    }

    public async Task EliminarAsync(int id)
    {
        var filasAfectadas = await _productoRepository.DeleteAsync(id);

        if (filasAfectadas == 0)
        {
            throw NoEncontrado(id);
        }
    }

    private async Task AsegurarExisteAsync(int id)
    {
        if (await _productoRepository.SelectByIdAsync(id) is null)
        {
            throw NoEncontrado(id);
        }
    }

    private async Task AsegurarNombreDisponibleAsync(string nombre, int? idExcluido)
    {
        if (await _productoRepository.ExisteNombreAsync(nombre, idExcluido))
        {
            throw new RegistroDuplicadoException($"Ya existe un producto con el nombre '{nombre}'.");
        }
    }

    private static RegistroNoEncontradoException NoEncontrado(int id)
    {
        return new RegistroNoEncontradoException($"No existe un producto con Id {id}.");
    }
}
