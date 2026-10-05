using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SaboresDelValle.Web.ModelBinding;

/// <summary>
/// Interpreta decimales con la cultura de la petición pero sin separador de miles,
/// para que en es-BO "35.50" no se convierta silenciosamente en 3550.
/// </summary>
public sealed class DecimalSinMilesModelBinder : IModelBinder
{
    private const NumberStyles EstilosPermitidos =
        NumberStyles.AllowLeadingWhite |
        NumberStyles.AllowTrailingWhite |
        NumberStyles.AllowLeadingSign |
        NumberStyles.AllowDecimalPoint;

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var nombre = bindingContext.ModelName;
        var valorRecibido = bindingContext.ValueProvider.GetValue(nombre);

        if (valorRecibido == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(nombre, valorRecibido);
        var texto = valorRecibido.FirstValue;

        if (string.IsNullOrWhiteSpace(texto))
        {
            VincularVacio(bindingContext, texto);
        }
        else if (decimal.TryParse(texto, EstilosPermitidos, valorRecibido.Culture, out var valor))
        {
            bindingContext.Result = ModelBindingResult.Success(valor);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(nombre, MensajeFormatoInvalido(bindingContext, texto, valorRecibido.Culture));
        }

        return Task.CompletedTask;
    }

    private static void VincularVacio(ModelBindingContext bindingContext, string? texto)
    {
        var metadata = bindingContext.ModelMetadata;

        if (metadata.IsReferenceOrNullableType)
        {
            bindingContext.Result = ModelBindingResult.Success(null);
            return;
        }

        bindingContext.ModelState.TryAddModelError(
            bindingContext.ModelName,
            metadata.ModelBindingMessageProvider.ValueMustNotBeNullAccessor(texto ?? string.Empty));
    }

    private static string MensajeFormatoInvalido(ModelBindingContext bindingContext, string texto, CultureInfo cultura)
    {
        var separador = cultura.NumberFormat.NumberDecimalSeparator;
        var ejemplo = 35.50m.ToString("0.00", cultura);

        return $"El valor '{texto}' no es válido para {bindingContext.ModelMetadata.GetDisplayName()}. " +
               $"Use '{separador}' como separador decimal y no use separador de miles (ejemplo: {ejemplo}).";
    }
}
