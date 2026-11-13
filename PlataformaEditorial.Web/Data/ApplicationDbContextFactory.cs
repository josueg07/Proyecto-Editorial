using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace PlataformaEditorial.Web.Data;
// Evita ejecutar la creación de roles al aplicar Identity en una base nueva.
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var basePath=File.Exists("appsettings.json")?Directory.GetCurrentDirectory():Path.Combine(Directory.GetCurrentDirectory(),"PlataformaEditorial.Web");
        var config=new ConfigurationBuilder().SetBasePath(basePath).AddJsonFile("appsettings.json").AddJsonFile("appsettings.Local.json",true).AddUserSecrets<ApplicationDbContextFactory>().AddEnvironmentVariables().Build();
        return new(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(config.GetConnectionString("DefaultConnection")??throw new InvalidOperationException("Falta la conexión DefaultConnection.")).Options);
    }
}
