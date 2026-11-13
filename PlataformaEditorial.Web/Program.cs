using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlataformaEditorial.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDbContext<PlataformaEditorial.Web.Data.Negocio.EditorialDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<PlataformaEditorial.Web.Models.Servicios.CatalogoService>();
builder.Services.AddScoped<PlataformaEditorial.Web.Models.Servicios.LibrosService>();
builder.Services.AddScoped<PlataformaEditorial.Web.Models.Servicios.ReferenciasService>();
builder.Services.AddScoped<PlataformaEditorial.Web.Models.Servicios.PresentacionesService>();

// RF-04: servicios independientes del catálogo y de sus mantenimientos.
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<PlataformaEditorial.Web.Models.Institucional.TiempoRf04>();
builder.Services.AddScoped<PlataformaEditorial.Web.Models.Servicios.Institucional.UnidadRf04>();
builder.Services.AddScoped<PlataformaEditorial.Web.Models.Servicios.Institucional.PerfilInstitucionalService>();
builder.Services.AddScoped<PlataformaEditorial.Web.Models.Servicios.Institucional.SolicitudesInstitucionalesService>();
builder.Services.AddScoped<PlataformaEditorial.Web.Models.Servicios.Institucional.CotizacionesInstitucionalesService>();
builder.Services.AddSingleton<PlataformaEditorial.Web.Models.Servicios.Institucional.CotizacionPdfService>();
builder.Services.AddScoped<PlataformaEditorial.Web.Models.Servicios.Institucional.IImpuestoInstitucional, PlataformaEditorial.Web.Models.Servicios.Institucional.ConfiguracionImpuestoService>();
// Proyecto académico: licencia Community de QuestPDF.
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddErrorDescriber<PlataformaEditorial.Web.Localizacion.ErroresIdentityEspanol>();
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender,PlataformaEditorial.Web.Localizacion.CorreoNoConfigurado>();
builder.Services.AddScoped<PlataformaEditorial.Web.Models.Servicios.Compras.UnidadCompra>();
builder.Services.AddScoped<PlataformaEditorial.Web.Models.Servicios.Compras.CarritoService>();
builder.Services.AddScoped<PlataformaEditorial.Web.Models.Servicios.Compras.PedidosService>();
builder.Services.AddLocalization();
builder.Services.AddControllersWithViews(options=>
{
    options.ModelMetadataDetailsProviders.Add(new PlataformaEditorial.Web.Localizacion.ValidacionEspanol());
    var m=options.ModelBindingMessageProvider;
    m.SetAttemptedValueIsInvalidAccessor((valor,campo)=>$"El valor de {campo} no es válido.");
    m.SetValueIsInvalidAccessor(valor=>"El valor no es válido.");
    m.SetValueMustBeANumberAccessor(campo=>$"El campo {campo} debe ser numérico.");
    m.SetValueMustNotBeNullAccessor(campo=>$"El campo {campo} es obligatorio.");
    m.SetMissingBindRequiredValueAccessor(campo=>$"Falta el campo {campo}.");
    m.SetNonPropertyAttemptedValueIsInvalidAccessor(valor=>"El valor no es válido.");
    m.SetNonPropertyUnknownValueIsInvalidAccessor(()=>"El valor no es válido.");
    m.SetNonPropertyValueMustBeANumberAccessor(()=>"El valor debe ser numérico.");
    m.SetUnknownValueIsInvalidAccessor(campo=>$"El valor de {campo} no es válido.");
    m.SetMissingKeyOrValueAccessor(()=>"Falta un valor obligatorio.");
    m.SetMissingRequestBodyRequiredValueAccessor(()=>"Faltan los datos del formulario.");
}).AddDataAnnotationsLocalization();

var app = builder.Build();
app.UseExceptionHandler("/Home/Error");

// Crear automáticamente los roles del sistema
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles =
    {
        "Administrador",
        "AsesorVentas",
        "ClienteMinorista",
        "ClienteInstitucional"
    };

    foreach (var rol in roles)
    {
        if (!await roleManager.RoleExistsAsync(rol))
        {
            var resultado = await roleManager.CreateAsync(
                new IdentityRole(rol));

            if (!resultado.Succeeded)
            {
                throw new Exception(
                    $"No se pudo crear el rol {rol}: " +
                    string.Join(", ", resultado.Errors.Select(e => e.Description)));
            }
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseRequestLocalization(new RequestLocalizationOptions().SetDefaultCulture("es-GT").AddSupportedCultures("es-GT").AddSupportedUICultures("es-GT"));
app.UseStatusCodePagesWithReExecute("/Errores/Estado/{0}");
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();

// Permite hospedar la aplicación real en las pruebas de integración.
public partial class Program { }
