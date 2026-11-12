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
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

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
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

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
