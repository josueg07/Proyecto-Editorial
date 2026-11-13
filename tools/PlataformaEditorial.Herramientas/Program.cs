using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlataformaEditorial.Web.Data;
using PlataformaEditorial.Web.Localizacion;

if(args.Length<2 || args[0] is not ("crear" or "confirmar"))
{
    Console.WriteLine("Herramienta local de administración académica (solo para el responsable de la base).");
    Console.WriteLine("crear <correo> <Administrador|AsesorVentas|ClienteMinorista>: solicita la contraseña de forma oculta, crea y confirma mediante Identity.");
    Console.WriteLine("confirmar <correo>: confirma una cuenta registrada, sin asignar roles institucionales.");
    return;
}
var root=Directory.GetCurrentDirectory();
var web=File.Exists(Path.Combine(root,"PlataformaEditorial.Web","appsettings.json"))?Path.Combine(root,"PlataformaEditorial.Web"):root;
var config=new ConfigurationBuilder().SetBasePath(web).AddJsonFile("appsettings.json").AddJsonFile("appsettings.Local.json",true).AddUserSecrets<ApplicationDbContextFactory>().AddEnvironmentVariables().Build();
var services=new ServiceCollection();
services.AddLogging(b=>b.SetMinimumLevel(LogLevel.Warning));
services.AddDbContext<ApplicationDbContext>(o=>o.UseSqlServer(Environment.GetEnvironmentVariable("EDITORIAL_CONNECTION")??config.GetConnectionString("DefaultConnection")));
services.AddDataProtection();
services.AddIdentityCore<IdentityUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders().AddErrorDescriber<ErroresIdentityEspanol>();
await using var provider=services.BuildServiceProvider();
await using var scope=provider.CreateAsyncScope();
var users=scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
var roles=scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
void Comprobar(IdentityResult result)
{
    if(!result.Succeeded) throw new InvalidOperationException(string.Join(" ",result.Errors.Select(e=>e.Description)));
}
try
{
    var user=await users.FindByEmailAsync(args[1]);
    if(args[0]=="crear")
    {
        if(args.Length!=3 || args[2] is not ("Administrador" or "AsesorVentas" or "ClienteMinorista")) throw new InvalidOperationException("Rol no permitido. El rol ClienteInstitucional solo se asigna al aprobar el perfil en la aplicación.");
        if(user!=null) throw new InvalidOperationException("La cuenta ya existe. No se modificó su contraseña ni sus roles.");
        if(Console.IsInputRedirected) throw new InvalidOperationException("Ejecuta en una terminal interactiva para introducir la contraseña de forma oculta.");
        Console.Write("Contraseña nueva (no se muestra): ");
        var password=new System.Text.StringBuilder();
        while(true)
        {
            var key=Console.ReadKey(true);
            if(key.Key==ConsoleKey.Enter) break;
            if(key.Key==ConsoleKey.Backspace) { if(password.Length>0) password.Length--; }
            else if(!char.IsControl(key.KeyChar)) password.Append(key.KeyChar);
        }
        Console.WriteLine();
        user=new IdentityUser { UserName=args[1],Email=args[1] };
        var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await using var transaction=await db.Database.BeginTransactionAsync();
        if(!await roles.RoleExistsAsync(args[2])) Comprobar(await roles.CreateAsync(new IdentityRole(args[2])));
        Comprobar(await users.CreateAsync(user,password.ToString()));password.Clear();
        Comprobar(await users.AddToRoleAsync(user,args[2]));
        var token=await users.GenerateEmailConfirmationTokenAsync(user);Comprobar(await users.ConfirmEmailAsync(user,token));
        await transaction.CommitAsync();
        Console.WriteLine("Cuenta de demostración creada y confirmada mediante Identity. No se envió correo.");
    }
    else
    {
        if(user==null) throw new InvalidOperationException("La cuenta no existe.");
        var token=await users.GenerateEmailConfirmationTokenAsync(user);Comprobar(await users.ConfirmEmailAsync(user,token));
        Console.WriteLine("Cuenta confirmada mediante Identity. No se cambiaron sus roles ni se envió correo.");
    }
}
catch(Exception ex)
{
    Console.Error.WriteLine(ex is InvalidOperationException?ex.Message:"No se pudo completar la operación. Revisa la conexión y la estructura de Identity.");
    Environment.ExitCode=1;
}
