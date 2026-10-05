namespace SaboresDelValle.Infrastructure.Persistence;

internal sealed class MapaCodigosSql<TEnum> where TEnum : struct, Enum
{
    private readonly string _columna;
    private readonly Dictionary<TEnum, string> _codigosPorValor = new();
    private readonly Dictionary<string, TEnum> _valoresPorCodigo = new(StringComparer.Ordinal);

    public MapaCodigosSql(string columna, params (TEnum Valor, string Codigo)[] pares)
    {
        _columna = columna;

        foreach (var (valor, codigo) in pares)
        {
            _codigosPorValor.Add(valor, codigo);
            _valoresPorCodigo.Add(codigo, valor);
        }

        var sinCodigo = Enum.GetValues<TEnum>().Where(v => !_codigosPorValor.ContainsKey(v)).ToArray();

        if (sinCodigo.Length > 0)
        {
            throw new InvalidOperationException(
                $"Faltan códigos SQL para {typeof(TEnum).Name} en '{columna}': {string.Join(", ", sinCodigo)}.");
        }
    }

    public string ACodigo(TEnum valor)
    {
        if (!_codigosPorValor.TryGetValue(valor, out var codigo))
        {
            throw new ArgumentOutOfRangeException(
                nameof(valor), valor, $"Valor sin código SQL para '{_columna}'.");
        }

        return codigo;
    }

    public TEnum AValor(string codigo)
    {
        if (!_valoresPorCodigo.TryGetValue(codigo, out var valor))
        {
            throw new InvalidOperationException(
                $"El código '{codigo}' almacenado en '{_columna}' no es reconocido.");
        }

        return valor;
    }
}
