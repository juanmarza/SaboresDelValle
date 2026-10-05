using System.Text;
using SaboresDelValle.Domain.Common;
using SaboresDelValle.Domain.Enums;
using SaboresDelValle.Domain.Exceptions;

namespace SaboresDelValle.Domain.Models;

public class Producto
{
    public const int NombreLongitudMaxima = 100;

    private const string LetrasEspecialesPermitidas = "ÑÁÉÍÓÚÜ";
    private const string SimbolosPermitidos = " .-/";

    public int Id { get; private set; }
    public string Nombre { get; private set; }
    public TipoProducto Tipo { get; private set; }
    public UnidadMedida Unidad { get; private set; }
    public decimal StockMinimo { get; private set; }
    public bool Activo { get; private set; }

    public Producto(string nombre, TipoProducto tipo, UnidadMedida unidad, decimal stockMinimo)
    {
        ValidarTipo(tipo);
        ValidarUnidad(unidad);

        Nombre = ValidarNombre(nombre);
        Tipo = tipo;
        Unidad = unidad;
        StockMinimo = ValidarStockMinimo(stockMinimo, unidad);
        Activo = true;
    }

    public Producto(int id, string nombre, TipoProducto tipo, UnidadMedida unidad,
                    decimal stockMinimo, bool activo)
        : this(nombre, tipo, unidad, stockMinimo)
    {
        if (id <= 0)
        {
            throw new DomainException("El identificador del producto debe ser mayor que cero.");
        }

        Id = id;
        Activo = activo;
    }

    public void CambiarNombre(string nombre)
    {
        Nombre = ValidarNombre(nombre);
    }

    public void CambiarTipo(TipoProducto tipo)
    {
        ValidarTipo(tipo);
        Tipo = tipo;
    }

    public void CambiarUnidad(UnidadMedida unidad)
    {
        ValidarUnidad(unidad);
        ValidarStockMinimo(StockMinimo, unidad);
        Unidad = unidad;
    }

    public void CambiarStockMinimo(decimal stockMinimo)
    {
        StockMinimo = ValidarStockMinimo(stockMinimo, Unidad);
    }

    public void Activar()
    {
        Activo = true;
    }

    public void Desactivar()
    {
        Activo = false;
    }

    public static string NormalizarNombre(string? nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return string.Empty;
        }

        var mayusculas = nombre.Normalize(NormalizationForm.FormC).Trim().ToUpperInvariant();

        return string.Join(' ', mayusculas.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string ValidarNombre(string? nombre)
    {
        var normalizado = NormalizarNombre(nombre);

        if (normalizado.Length == 0)
        {
            throw new DomainException("El nombre del producto es obligatorio.");
        }

        if (normalizado.Length > NombreLongitudMaxima)
        {
            throw new DomainException(
                $"El nombre del producto no puede superar {NombreLongitudMaxima} caracteres.");
        }

        var caracteresInvalidos = normalizado.Where(c => !EsCaracterPermitido(c)).Distinct().ToArray();

        if (caracteresInvalidos.Length > 0)
        {
            throw new DomainException(
                $"El nombre del producto contiene caracteres no permitidos: '{string.Join("', '", caracteresInvalidos)}'.");
        }

        if (!normalizado.Any(EsLetra))
        {
            throw new DomainException("El nombre del producto debe contener al menos una letra.");
        }

        return normalizado;
    }

    private static bool EsLetra(char c)
    {
        return (c >= 'A' && c <= 'Z') || LetrasEspecialesPermitidas.Contains(c);
    }

    private static bool EsCaracterPermitido(char c)
    {
        return EsLetra(c) || (c >= '0' && c <= '9') || SimbolosPermitidos.Contains(c);
    }

    private static void ValidarTipo(TipoProducto tipo)
    {
        if (!Enum.IsDefined(tipo))
        {
            throw new DomainException("El tipo de producto no es válido.");
        }
    }

    private static void ValidarUnidad(UnidadMedida unidad)
    {
        if (!Enum.IsDefined(unidad))
        {
            throw new DomainException("La unidad de medida no es válida.");
        }
    }

    private static decimal ValidarStockMinimo(decimal stockMinimo, UnidadMedida unidad)
    {
        if (stockMinimo < 0)
        {
            throw new DomainException("El stock mínimo no puede ser negativo.");
        }

        var redondeado = PrecisionPolicy.Redondear(stockMinimo);

        if (unidad == UnidadMedida.Unidad && decimal.Truncate(redondeado) != redondeado)
        {
            throw new DomainException(
                "El stock mínimo debe ser un número entero cuando la unidad de medida es Unidad.");
        }

        return redondeado;
    }
}
