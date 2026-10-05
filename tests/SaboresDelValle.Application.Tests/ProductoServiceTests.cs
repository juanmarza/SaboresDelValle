using SaboresDelValle.Application.Exceptions;
using SaboresDelValle.Application.Services;
using SaboresDelValle.Application.Tests.Fakes;
using SaboresDelValle.Domain.Enums;
using SaboresDelValle.Domain.Exceptions;
using SaboresDelValle.Domain.Models;

namespace SaboresDelValle.Application.Tests;

public class ProductoServiceTests
{
    private readonly ProductoRepositoryEnMemoria _repositorio = new();
    private readonly ProductoService _servicio;

    public ProductoServiceTests()
    {
        _servicio = new ProductoService(_repositorio);
    }

    private Producto Existente(string nombre, UnidadMedida unidad = UnidadMedida.Kilogramo, decimal stock = 1m)
    {
        return _repositorio.Agregar(new Producto(nombre, TipoProducto.NoPerecedero, unidad, stock));
    }

    [Fact]
    public async Task CrearAsync_DatosValidos_GuardaProductoNormalizadoYDevuelveId()
    {
        var id = await _servicio.CrearAsync("  arroz   blanco ", TipoProducto.NoPerecedero, UnidadMedida.Kilogramo, 2.5m);

        var guardado = await _servicio.ObtenerPorIdAsync(id);
        Assert.NotNull(guardado);
        Assert.Equal("ARROZ BLANCO", guardado.Nombre);
        Assert.True(guardado.Activo);
    }

    [Fact]
    public async Task CrearAsync_NombreDuplicado_LanzaRegistroDuplicadoException()
    {
        Existente("ARROZ");

        await Assert.ThrowsAsync<RegistroDuplicadoException>(() =>
            _servicio.CrearAsync("arroz", TipoProducto.NoPerecedero, UnidadMedida.Kilogramo, 1m));
        Assert.Equal(1, _repositorio.Cantidad);
    }

    [Fact]
    public async Task CrearAsync_NombreDeProductoInactivo_LanzaRegistroDuplicadoException()
    {
        var inactivo = Existente("ARROZ");
        inactivo.Desactivar();
        await _repositorio.UpdateAsync(inactivo);

        await Assert.ThrowsAsync<RegistroDuplicadoException>(() =>
            _servicio.CrearAsync("Arroz", TipoProducto.NoPerecedero, UnidadMedida.Kilogramo, 1m));
    }

    [Fact]
    public async Task CrearAsync_DatosInvalidos_LanzaDomainExceptionYNoGuarda()
    {
        await Assert.ThrowsAsync<DomainException>(() =>
            _servicio.CrearAsync("Sal", TipoProducto.NoPerecedero, UnidadMedida.Kilogramo, -1m));
        Assert.Equal(0, _repositorio.Cantidad);
    }

    [Fact]
    public async Task EditarAsync_ConservandoSuPropioNombre_ActualizaProducto()
    {
        var huevo = Existente("HUEVO", UnidadMedida.Unidad, 3m);

        await _servicio.EditarAsync(huevo.Id, "Huevo", TipoProducto.Perecedero, UnidadMedida.Unidad, 6m, activo: false);

        var editado = await _servicio.ObtenerPorIdAsync(huevo.Id);
        Assert.Equal(TipoProducto.Perecedero, editado!.Tipo);
        Assert.Equal(6m, editado.StockMinimo);
        Assert.False(editado.Activo);
    }

    [Fact]
    public async Task EditarAsync_NombreDeOtroProducto_LanzaRegistroDuplicadoException()
    {
        Existente("ARROZ");
        var huevo = Existente("HUEVO");

        await Assert.ThrowsAsync<RegistroDuplicadoException>(() =>
            _servicio.EditarAsync(huevo.Id, "arroz", TipoProducto.NoPerecedero, UnidadMedida.Kilogramo, 1m, true));
    }

    [Fact]
    public async Task EditarAsync_CambiaUnidadYStockALaVez_ActualizaProducto()
    {
        var arroz = Existente("ARROZ", UnidadMedida.Kilogramo, 2.5m);

        await _servicio.EditarAsync(arroz.Id, "ARROZ", TipoProducto.NoPerecedero, UnidadMedida.Unidad, 3m, true);

        var editado = await _servicio.ObtenerPorIdAsync(arroz.Id);
        Assert.Equal(UnidadMedida.Unidad, editado!.Unidad);
        Assert.Equal(3m, editado.StockMinimo);
    }

    [Fact]
    public async Task EditarAsync_IdInexistente_LanzaRegistroNoEncontradoException()
    {
        await Assert.ThrowsAsync<RegistroNoEncontradoException>(() =>
            _servicio.EditarAsync(999, "SAL", TipoProducto.NoPerecedero, UnidadMedida.Kilogramo, 1m, true));
    }

    [Fact]
    public async Task EliminarAsync_IdExistente_EliminaProducto()
    {
        var arroz = Existente("ARROZ");

        await _servicio.EliminarAsync(arroz.Id);

        Assert.Null(await _servicio.ObtenerPorIdAsync(arroz.Id));
    }

    [Fact]
    public async Task EliminarAsync_IdInexistente_LanzaRegistroNoEncontradoException()
    {
        await Assert.ThrowsAsync<RegistroNoEncontradoException>(() => _servicio.EliminarAsync(999));
    }
}
