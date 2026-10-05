using SaboresDelValle.Domain.Enums;
using SaboresDelValle.Domain.Exceptions;
using SaboresDelValle.Domain.Models;

namespace SaboresDelValle.Domain.Tests;

public class PlatoTests
{
    private static Plato CrearPlato(string nombre = "Sopa de maní", decimal precio = 15m, TipoPlato tipo = TipoPlato.Sopa)
    {
        return new Plato(nombre, precio, tipo);
    }

    [Fact]
    public void Constructor_DatosValidos_CreaPlatoDisponible()
    {
        var plato = CrearPlato();

        Assert.True(plato.Disponible);
        Assert.Equal(15m, plato.Precio);
        Assert.Equal(TipoPlato.Sopa, plato.Tipo);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_NombreVacio_LanzaDomainException(string? nombre)
    {
        Assert.Throws<DomainException>(() => CrearPlato(nombre: nombre!));
    }

    [Fact]
    public void Constructor_NombreDemasiadoLargo_LanzaDomainException()
    {
        var nombre = new string('a', Plato.NombreLongitudMaxima + 1);

        Assert.Throws<DomainException>(() => CrearPlato(nombre: nombre));
    }

    [Fact]
    public void Constructor_NombreConEspaciosSobrantes_SeRecortaSinCambiarMayusculas()
    {
        Assert.Equal("Sopa de maní", CrearPlato(nombre: "  Sopa   de maní ").Nombre);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_PrecioNoPositivo_LanzaDomainException(decimal precio)
    {
        Assert.Throws<DomainException>(() => CrearPlato(precio: precio));
    }

    public static TheoryData<decimal, decimal> PreciosConMasDeDosDecimales => new()
    {
        { 35.564m, 35.56m },
        { 35.565m, 35.57m },
        { 35.575m, 35.58m },
        { 10.555m, 10.56m }
    };

    [Theory]
    [MemberData(nameof(PreciosConMasDeDosDecimales))]
    public void Constructor_PrecioConMasDeDosDecimales_SeRedondeaAwayFromZero(decimal precio, decimal esperado)
    {
        Assert.Equal(esperado, CrearPlato(precio: precio).Precio);
    }

    [Theory]
    [MemberData(nameof(NegativosPequenos))]
    public void Constructor_PrecioNegativoPequeno_LanzaDomainException(decimal precio)
    {
        Assert.Throws<DomainException>(() => CrearPlato(precio: precio));
    }

    public static TheoryData<decimal> NegativosPequenos => new() { -0.004m, -0.005m };

    [Fact]
    public void Constructor_PrecioQueRedondeaACero_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => CrearPlato(precio: 0.004m));
    }

    [Fact]
    public void Constructor_PrecioQueRedondeaAUnCentavo_SeAcepta()
    {
        Assert.Equal(0.01m, CrearPlato(precio: 0.005m).Precio);
    }

    [Fact]
    public void CambiarPrecio_ConMasDeDosDecimales_SeRedondea()
    {
        var plato = CrearPlato();

        plato.CambiarPrecio(100m / 6m);

        Assert.Equal(16.67m, plato.Precio);
    }

    [Fact]
    public void Constructor_TipoInvalido_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => CrearPlato(tipo: (TipoPlato)99));
    }

    [Fact]
    public void MarcarNoDisponible_PlatoDisponible_QuedaNoDisponible()
    {
        var plato = CrearPlato();

        plato.MarcarNoDisponible();

        Assert.False(plato.Disponible);
    }

    [Fact]
    public void CambiarNombre_NombreValido_ActualizaNombreNormalizado()
    {
        var plato = CrearPlato();

        plato.CambiarNombre("  Pique   macho ");

        Assert.Equal("Pique macho", plato.Nombre);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CambiarNombre_NombreVacio_LanzaDomainExceptionYConservaNombre(string nombre)
    {
        var plato = CrearPlato(nombre: "Sopa de maní");

        Assert.Throws<DomainException>(() => plato.CambiarNombre(nombre));
        Assert.Equal("Sopa de maní", plato.Nombre);
    }

    [Fact]
    public void CambiarTipo_TipoValido_ActualizaTipo()
    {
        var plato = CrearPlato(tipo: TipoPlato.Sopa);

        plato.CambiarTipo(TipoPlato.Segundo);

        Assert.Equal(TipoPlato.Segundo, plato.Tipo);
    }

    [Fact]
    public void CambiarTipo_TipoInvalido_LanzaDomainExceptionYConservaTipo()
    {
        var plato = CrearPlato(tipo: TipoPlato.Sopa);

        Assert.Throws<DomainException>(() => plato.CambiarTipo((TipoPlato)99));
        Assert.Equal(TipoPlato.Sopa, plato.Tipo);
    }

    [Fact]
    public void MarcarDisponible_PlatoNoDisponible_QuedaDisponible()
    {
        var plato = CrearPlato();
        plato.MarcarNoDisponible();

        plato.MarcarDisponible();

        Assert.True(plato.Disponible);
    }

    [Fact]
    public void ConstructorConId_NoDisponible_ConservaDisponibilidadYPrecioRedondeado()
    {
        var plato = new Plato(7, "Pique macho", 37.505m, TipoPlato.Segundo, disponible: false);

        Assert.Equal(7, plato.Id);
        Assert.False(plato.Disponible);
        Assert.Equal(37.51m, plato.Precio);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ConstructorConId_IdNoPositivo_LanzaDomainException(int id)
    {
        Assert.Throws<DomainException>(() => new Plato(id, "Pique", 35m, TipoPlato.Segundo, true));
    }

    [Fact]
    public void CambiarPrecio_PrecioCero_LanzaDomainExceptionYConservaPrecio()
    {
        var plato = CrearPlato(precio: 15m);

        Assert.Throws<DomainException>(() => plato.CambiarPrecio(0m));
        Assert.Equal(15m, plato.Precio);
    }
}
