namespace SaboresDelValle.Domain.Common;

/// <summary>
/// Política de precisión del negocio: todo importe o cantidad decimal
/// se trabaja con 2 decimales y redondeo comercial (0,5 se aleja de cero).
/// </summary>
public static class PrecisionPolicy
{
    public const int Decimales = 2;

    public static decimal Redondear(decimal valor)
    {
        return Math.Round(valor, Decimales, MidpointRounding.AwayFromZero);
    }
}
