using System.Globalization;
using Microsoft.AspNetCore.Localization;
using SaboresDelValle.Application;
using SaboresDelValle.Infrastructure;
using SaboresDelValle.Web.ModelBinding;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages()
    .AddMvcOptions(options =>
    {
        options.ModelBinderProviders.Insert(0, new DecimalSinMilesModelBinderProvider());

        var mensajes = options.ModelBindingMessageProvider;
        mensajes.SetAttemptedValueIsInvalidAccessor((valor, campo) => $"El valor '{valor}' no es válido para {campo}.");
        mensajes.SetValueIsInvalidAccessor(valor => $"El valor '{valor}' no es válido.");
        mensajes.SetValueMustBeANumberAccessor(campo => $"El campo {campo} debe ser un número.");
        mensajes.SetValueMustNotBeNullAccessor(campo => $"El campo {campo} es obligatorio.");
    });
builder.Services.AddApplication();

// Cultura de Bolivia para toda la aplicación: 1.250,50 y Bs. 1.250,50.
var culturaBolivia = new CultureInfo("es-BO");
culturaBolivia.NumberFormat.CurrencySymbol = "Bs.";
culturaBolivia.NumberFormat.CurrencyPositivePattern = 2;
culturaBolivia.NumberFormat.CurrencyNegativePattern = 9;
culturaBolivia = CultureInfo.ReadOnly(culturaBolivia);

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(culturaBolivia);
    options.SupportedCultures = [culturaBolivia];
    options.SupportedUICultures = [culturaBolivia];
    options.RequestCultureProviders.Clear();
});
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRequestLocalization();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
