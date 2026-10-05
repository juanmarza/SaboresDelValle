using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SaboresDelValle.Web.ModelBinding;

public sealed class DecimalSinMilesModelBinderProvider : IModelBinderProvider
{
    private static readonly DecimalSinMilesModelBinder Binder = new();

    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        var tipo = Nullable.GetUnderlyingType(context.Metadata.ModelType) ?? context.Metadata.ModelType;

        return tipo == typeof(decimal) ? Binder : null;
    }
}
