using SaboresDelValle.Application.Exceptions;
using SaboresDelValle.Application.Services;
using SaboresDelValle.Application.Tests.Fakes;
using SaboresDelValle.Domain.Enums;
using SaboresDelValle.Domain.Exceptions;
using SaboresDelValle.Domain.Models;

namespace SaboresDelValle.Application.Tests;

public class PlatoServiceTests
{
    private readonly PlatoRepositoryEnMemoria _repositorio = new();
    private readonly PlatoService _servicio;

    public PlatoServiceTests()
    {
        _servicio = new PlatoService(_repositorio);
    }

    private Plato Existente(string nombre, decimal precio = 15m)
    {
        return _repositorio.Agregar(new Plato(nombre, precio, TipoPlato.Sopa));
    }

    [Fact]
    public async Task CrearAsync_DatosValidos_GuardaPlatoDisponibleYDevuelveId()
    {
        var id = await _servicio.CrearAsync("  Sopa de maní ", 15m, TipoPlato.Sopa);

        var guardado = await _servicio.ObtenerPorIdAsync(id);
        Assert.NotNull(guardado);
        Assert.Equal("Sopa de maní", guardado.Nombre);
        Assert.True(guardado.Disponible);
    }

    [Fact]
    public async Task CrearAsync_PrecioConMasDeDosDecimales_GuardaPrecioRedondeado()
    {
        var id = await _servicio.CrearAsync("Pique macho", 35.565m, TipoPlato.Segundo);

        Assert.Equal(35.57m, (await _servicio.ObtenerPorIdAsync(id))!.Precio);
    }

    [Fact]
    public async Task CrearAsync_NombreDuplicado_LanzaRegistroDuplicadoException()
    {
        Existente("Sopa de maní");

        await Assert.ThrowsAsync<RegistroDuplicadoException>(() =>
            _servicio.CrearAsync("SOPA DE MANÍ", 10m, TipoPlato.Sopa));
        Assert.Equal(1, _repositorio.Cantidad);
    }

    [Fact]
    public async Task CrearAsync_PrecioCero_LanzaDomainExceptionYNoGuarda()
    {
        await Assert.ThrowsAsync<DomainException>(() => _servicio.CrearAsync("Pique", 0m, TipoPlato.Segundo));
        Assert.Equal(0, _repositorio.Cantidad);
    }

    [Fact]
    public async Task EditarAsync_DatosValidos_ActualizaPlato()
    {
        var sopa = Existente("Sopa de maní", 15m);

        await _servicio.EditarAsync(sopa.Id, "Sopa de maní", 18m, TipoPlato.Sopa, disponible: false);

        var editado = await _servicio.ObtenerPorIdAsync(sopa.Id);
        Assert.Equal(18m, editado!.Precio);
        Assert.False(editado.Disponible);
    }

    [Fact]
    public async Task EditarAsync_ConservandoSuPropioNombre_ActualizaPlato()
    {
        var pique = Existente("Pique macho", 35m);

        await _servicio.EditarAsync(pique.Id, "PIQUE MACHO", 37.505m, TipoPlato.Segundo, disponible: true);

        var editado = await _servicio.ObtenerPorIdAsync(pique.Id);
        Assert.Equal("PIQUE MACHO", editado!.Nombre);
        Assert.Equal(37.51m, editado.Precio);
    }

    [Fact]
    public async Task EditarAsync_CambiaNombreYTipo_ActualizaPlato()
    {
        var plato = Existente("Pique", 35m);

        await _servicio.EditarAsync(plato.Id, "Pique macho", 35m, TipoPlato.Extra, disponible: true);

        var editado = await _servicio.ObtenerPorIdAsync(plato.Id);
        Assert.Equal("Pique macho", editado!.Nombre);
        Assert.Equal(TipoPlato.Extra, editado.Tipo);
    }

    [Fact]
    public async Task EditarAsync_PlatoNoDisponibleAMarcarDisponible_QuedaDisponible()
    {
        var plato = Existente("Pique", 35m);
        await _servicio.EditarAsync(plato.Id, "Pique", 35m, TipoPlato.Sopa, disponible: false);

        await _servicio.EditarAsync(plato.Id, "Pique", 35m, TipoPlato.Sopa, disponible: true);

        Assert.True((await _servicio.ObtenerPorIdAsync(plato.Id))!.Disponible);
    }

    [Fact]
    public async Task EditarAsync_PrecioInvalido_LanzaDomainExceptionYNoModificaPlato()
    {
        var plato = Existente("Pique", 35m);

        await Assert.ThrowsAsync<DomainException>(() =>
            _servicio.EditarAsync(plato.Id, "Pique", 0.004m, TipoPlato.Sopa, disponible: true));

        Assert.Equal(35m, (await _servicio.ObtenerPorIdAsync(plato.Id))!.Precio);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_IdExistente_DevuelvePlato()
    {
        var plato = Existente("Pique", 35m);

        var obtenido = await _servicio.ObtenerPorIdAsync(plato.Id);

        Assert.NotNull(obtenido);
        Assert.Equal("Pique", obtenido.Nombre);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_IdInexistente_DevuelveNull()
    {
        Assert.Null(await _servicio.ObtenerPorIdAsync(999));
    }

    [Fact]
    public async Task ListarAsync_ConPlatos_DevuelveTodos()
    {
        Existente("Sopa de maní");
        Existente("Pique macho");

        Assert.Equal(2, (await _servicio.ListarAsync()).Count);
    }

    [Fact]
    public async Task EditarAsync_NombreDeOtroPlato_LanzaRegistroDuplicadoException()
    {
        Existente("Sopa de maní");
        var pique = Existente("Pique macho");

        await Assert.ThrowsAsync<RegistroDuplicadoException>(() =>
            _servicio.EditarAsync(pique.Id, "sopa de maní", 45m, TipoPlato.Segundo, true));
    }

    [Fact]
    public async Task EditarAsync_IdInexistente_LanzaRegistroNoEncontradoException()
    {
        await Assert.ThrowsAsync<RegistroNoEncontradoException>(() =>
            _servicio.EditarAsync(999, "Pique", 45m, TipoPlato.Segundo, true));
    }

    [Fact]
    public async Task EliminarAsync_IdExistente_EliminaPlato()
    {
        var sopa = Existente("Sopa de maní");

        await _servicio.EliminarAsync(sopa.Id);

        Assert.Null(await _servicio.ObtenerPorIdAsync(sopa.Id));
    }

    [Fact]
    public async Task EliminarAsync_IdInexistente_LanzaRegistroNoEncontradoException()
    {
        await Assert.ThrowsAsync<RegistroNoEncontradoException>(() => _servicio.EliminarAsync(999));
    }
}
