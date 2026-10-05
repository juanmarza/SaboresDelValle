using SaboresDelValle.Domain.Enums;
using SaboresDelValle.Domain.Exceptions;
using SaboresDelValle.Domain.Models;

namespace SaboresDelValle.Domain.Tests;

public class ProductoTests
{
    private static Producto CrearProducto(
        string nombre = "Arroz",
        TipoProducto tipo = TipoProducto.NoPerecedero,
        UnidadMedida unidad = UnidadMedida.Kilogramo,
        decimal stockMinimo = 1m)
    {
        return new Producto(nombre, tipo, unidad, stockMinimo);
    }

    [Fact]
    public void Constructor_DatosValidos_CreaProductoActivo()
    {
        var producto = CrearProducto();

        Assert.True(producto.Activo);
        Assert.Equal(TipoProducto.NoPerecedero, producto.Tipo);
        Assert.Equal(UnidadMedida.Kilogramo, producto.Unidad);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_NombreVacio_LanzaDomainException(string? nombre)
    {
        Assert.Throws<DomainException>(() => CrearProducto(nombre: nombre!));
    }

    [Fact]
    public void Constructor_NombreDemasiadoLargo_LanzaDomainException()
    {
        var nombre = new string('A', Producto.NombreLongitudMaxima + 1);

        Assert.Throws<DomainException>(() => CrearProducto(nombre: nombre));
    }

    [Fact]
    public void Constructor_NombreConLongitudMaxima_SeAcepta()
    {
        var nombre = new string('A', Producto.NombreLongitudMaxima);

        Assert.Equal(nombre, CrearProducto(nombre: nombre).Nombre);
    }

    [Theory]
    [InlineData("  arroz   blanco ", "ARROZ BLANCO")]
    [InlineData("piña ácida", "PIÑA ÁCIDA")]
    [InlineData("aceite 1/2 l.", "ACEITE 1/2 L.")]
    public void Constructor_Nombre_SeNormalizaAMayusculasSinEspaciosSobrantes(string nombre, string esperado)
    {
        Assert.Equal(esperado, CrearProducto(nombre: nombre).Nombre);
    }

    [Theory]
    [InlineData("ARROZ@")]
    [InlineData("SAL#FINA")]
    [InlineData("AZÚCAR*")]
    public void Constructor_NombreConCaracteresNoPermitidos_LanzaDomainException(string nombre)
    {
        Assert.Throws<DomainException>(() => CrearProducto(nombre: nombre));
    }

    [Fact]
    public void Constructor_NombreSinLetras_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => CrearProducto(nombre: "123"));
    }

    [Fact]
    public void Constructor_TipoInvalido_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => CrearProducto(tipo: (TipoProducto)99));
    }

    [Fact]
    public void Constructor_UnidadInvalida_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => CrearProducto(unidad: (UnidadMedida)99));
    }

    [Fact]
    public void Constructor_StockMinimoNegativo_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => CrearProducto(stockMinimo: -0.01m));
    }

    public static TheoryData<decimal> NegativosPequenos => new() { -0.004m, -0.005m };

    [Theory]
    [MemberData(nameof(NegativosPequenos))]
    public void Constructor_StockNegativoPequeno_LanzaDomainExceptionSinRedondearACero(decimal stockMinimo)
    {
        Assert.Throws<DomainException>(() => CrearProducto(stockMinimo: stockMinimo));
    }

    [Theory]
    [MemberData(nameof(NegativosPequenos))]
    public void CambiarStockMinimo_NegativoPequeno_LanzaDomainExceptionYConservaValor(decimal stockMinimo)
    {
        var producto = CrearProducto(stockMinimo: 5m);

        Assert.Throws<DomainException>(() => producto.CambiarStockMinimo(stockMinimo));
        Assert.Equal(5m, producto.StockMinimo);
    }

    [Fact]
    public void Constructor_UnidadConStockDecimal_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => CrearProducto(unidad: UnidadMedida.Unidad, stockMinimo: 2.5m));
    }

    [Theory]
    [InlineData(UnidadMedida.Kilogramo, 2.5)]
    [InlineData(UnidadMedida.Litro, 0.75)]
    [InlineData(UnidadMedida.Unidad, 3)]
    public void Constructor_StockSegunUnidad_SeAcepta(UnidadMedida unidad, decimal stockMinimo)
    {
        Assert.Equal(stockMinimo, CrearProducto(unidad: unidad, stockMinimo: stockMinimo).StockMinimo);
    }

    public static TheoryData<decimal, decimal> StocksConMasDeDosDecimales => new()
    {
        { 1.2345m, 1.23m },
        { 2.555m, 2.56m },
        { 8.333333m, 8.33m }
    };

    [Theory]
    [MemberData(nameof(StocksConMasDeDosDecimales))]
    public void Constructor_StockConMasDeDosDecimales_SeRedondeaADosDecimales(decimal stock, decimal esperado)
    {
        Assert.Equal(esperado, CrearProducto(stockMinimo: stock).StockMinimo);
    }

    [Fact]
    public void Constructor_UnidadConStockQueRedondeaAEntero_SeAcepta()
    {
        Assert.Equal(2m, CrearProducto(unidad: UnidadMedida.Unidad, stockMinimo: 2.004m).StockMinimo);
    }

    [Fact]
    public void Constructor_UnidadConStockQueRedondeaADecimal_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => CrearProducto(unidad: UnidadMedida.Unidad, stockMinimo: 2.005m));
    }

    [Fact]
    public void CambiarStockMinimo_ConMasDeDosDecimales_SeRedondea()
    {
        var producto = CrearProducto();

        producto.CambiarStockMinimo(25m / 3m);

        Assert.Equal(8.33m, producto.StockMinimo);
    }

    [Fact]
    public void CambiarUnidad_AUnidadConStockDecimalActual_LanzaDomainException()
    {
        var producto = CrearProducto(unidad: UnidadMedida.Kilogramo, stockMinimo: 2.5m);

        Assert.Throws<DomainException>(() => producto.CambiarUnidad(UnidadMedida.Unidad));
        Assert.Equal(UnidadMedida.Kilogramo, producto.Unidad);
    }

    [Fact]
    public void Desactivar_ProductoActivo_QuedaInactivo()
    {
        var producto = CrearProducto();

        producto.Desactivar();

        Assert.False(producto.Activo);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_IdNoPositivo_LanzaDomainException(int id)
    {
        Assert.Throws<DomainException>(() =>
            new Producto(id, "Arroz", TipoProducto.NoPerecedero, UnidadMedida.Kilogramo, 1m, true));
    }
}
