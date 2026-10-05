using SaboresDelValle.Domain.Common;

namespace SaboresDelValle.Domain.Tests;

public class PrecisionPolicyTests
{
    public static TheoryData<decimal, decimal> Casos => new()
    {
        { 35.564m, 35.56m },
        { 35.565m, 35.57m },
        { 35.574m, 35.57m },
        { 35.575m, 35.58m },
        { 35.576m, 35.58m },
        { 10m / 3m, 3.33m },
        { 100m / 6m, 16.67m },
        { 25m / 3m, 8.33m },
        { 0m, 0m },
        { -35.565m, -35.57m },
        { 1250.5m, 1250.50m }
    };

    [Theory]
    [MemberData(nameof(Casos))]
    public void Redondear_Valor_DevuelveDosDecimalesConAwayFromZero(decimal valor, decimal esperado)
    {
        Assert.Equal(esperado, PrecisionPolicy.Redondear(valor));
    }

    // Con ToEven (redondeo bancario) estos puntos medios bajarían: 0,12 y 2,34.
    public static TheoryData<decimal, decimal> PuntosMediosQueToEvenRedondeariaHaciaAbajo => new()
    {
        { 0.125m, 0.13m },
        { 2.345m, 2.35m }
    };

    [Theory]
    [MemberData(nameof(PuntosMediosQueToEvenRedondeariaHaciaAbajo))]
    public void Redondear_PuntoMedio_NoUsaRedondeoBancario(decimal valor, decimal esperado)
    {
        Assert.Equal(esperado, PrecisionPolicy.Redondear(valor));
    }

    [Fact]
    public void Redondear_Division_NoConservaPrecisionIlimitada()
    {
        var resultado = PrecisionPolicy.Redondear(10m / 3m);

        Assert.Equal(PrecisionPolicy.Decimales, resultado.Scale);
    }
}
