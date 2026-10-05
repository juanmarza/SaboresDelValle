using System.Text;
using SaboresDelValle.Domain.Common;
using SaboresDelValle.Domain.Enums;
using SaboresDelValle.Domain.Exceptions;

namespace SaboresDelValle.Domain.Models;

public class Plato
{
    public const int NombreLongitudMaxima = 100;

    public int Id { get; private set; }
    public string Nombre { get; private set; }
    public decimal Precio { get; private set; }
    public TipoPlato Tipo { get; private set; }
    public bool Disponible { get; private set; }

    public Plato(string nombre, decimal precio, TipoPlato tipo)
    {
        ValidarTipo(tipo);

        Nombre = ValidarNombre(nombre);
        Precio = ValidarPrecio(precio);
        Tipo = tipo;
        Disponible = true;
    }

    public Plato(int id, string nombre, decimal precio, TipoPlato tipo, bool disponible)
        : this(nombre, precio, tipo)
    {
        if (id <= 0)
        {
            throw new DomainException("El identificador del plato debe ser mayor que cero.");
        }

        Id = id;
        Disponible = disponible;
    }

    public void CambiarNombre(string nombre)
    {
        Nombre = ValidarNombre(nombre);
    }

    public void CambiarPrecio(decimal precio)
    {
        Precio = ValidarPrecio(precio);
    }

    public void CambiarTipo(TipoPlato tipo)
    {
        ValidarTipo(tipo);
        Tipo = tipo;
    }

    public void MarcarDisponible()
    {
        Disponible = true;
    }

    public void MarcarNoDisponible()
    {
        Disponible = false;
    }

    public static string NormalizarNombre(string? nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return string.Empty;
        }

        var limpio = nombre.Normalize(NormalizationForm.FormC).Trim();

        return string.Join(' ', limpio.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string ValidarNombre(string? nombre)
    {
        var normalizado = NormalizarNombre(nombre);

        if (normalizado.Length == 0)
        {
            throw new DomainException("El nombre del plato es obligatorio.");
        }

        if (normalizado.Length > NombreLongitudMaxima)
        {
            throw new DomainException(
                $"El nombre del plato no puede superar {NombreLongitudMaxima} caracteres.");
        }

        return normalizado;
    }

    private static decimal ValidarPrecio(decimal precio)
    {
        const string mensaje = "El precio del plato debe ser mayor que cero.";

        if (precio <= 0)
        {
            throw new DomainException(mensaje);
        }

        var redondeado = PrecisionPolicy.Redondear(precio);

        if (redondeado <= 0)
        {
            throw new DomainException(mensaje);
        }

        return redondeado;
    }

    private static void ValidarTipo(TipoPlato tipo)
    {
        if (!Enum.IsDefined(tipo))
        {
            throw new DomainException("El tipo de plato es obligatorio y debe ser válido.");
        }
    }
}
