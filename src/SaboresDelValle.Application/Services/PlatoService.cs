using SaboresDelValle.Application.Exceptions;
using SaboresDelValle.Application.Interfaces;
using SaboresDelValle.Domain.Enums;
using SaboresDelValle.Domain.Models;

namespace SaboresDelValle.Application.Services;

public class PlatoService : IPlatoService
{
    private readonly IPlatoRepository _platoRepository;

    public PlatoService(IPlatoRepository platoRepository)
    {
        _platoRepository = platoRepository;
    }

    public Task<List<Plato>> ListarAsync()
    {
        return _platoRepository.SelectAllAsync();
    }

    public Task<Plato?> ObtenerPorIdAsync(int id)
    {
        return _platoRepository.SelectByIdAsync(id);
    }

    public async Task<int> CrearAsync(string nombre, decimal precio, TipoPlato tipo)
    {
        var plato = new Plato(nombre, precio, tipo);

        await AsegurarNombreDisponibleAsync(plato.Nombre, idExcluido: null);

        return await _platoRepository.InsertAsync(plato);
    }

    public async Task EditarAsync(int id, string nombre, decimal precio, TipoPlato tipo, bool disponible)
    {
        var plato = new Plato(id, nombre, precio, tipo, disponible);

        await AsegurarExisteAsync(id);
        await AsegurarNombreDisponibleAsync(plato.Nombre, idExcluido: id);

        await _platoRepository.UpdateAsync(plato);
    }

    public async Task EliminarAsync(int id)
    {
        var filasAfectadas = await _platoRepository.DeleteAsync(id);

        if (filasAfectadas == 0)
        {
            throw NoEncontrado(id);
        }
    }

    private async Task AsegurarExisteAsync(int id)
    {
        if (await _platoRepository.SelectByIdAsync(id) is null)
        {
            throw NoEncontrado(id);
        }
    }

    private async Task AsegurarNombreDisponibleAsync(string nombre, int? idExcluido)
    {
        if (await _platoRepository.ExisteNombreAsync(nombre, idExcluido))
        {
            throw new RegistroDuplicadoException($"Ya existe un plato con el nombre '{nombre}'.");
        }
    }

    private static RegistroNoEncontradoException NoEncontrado(int id)
    {
        return new RegistroNoEncontradoException($"No existe un plato con Id {id}.");
    }
}
