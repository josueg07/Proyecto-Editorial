using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlataformaEditorial.Web.Data.Negocio;
using PlataformaEditorial.Web.Models.Compras;
using PlataformaEditorial.Web.Models.Entidades;
using PlataformaEditorial.Web.Models.Servicios.Compras;
using System.Net;
using static PlataformaEditorial.Tests.InstitucionalEscenario;
namespace PlataformaEditorial.Tests;
public class ComprasTests
{
    [Theory]
    [InlineData(5,0)] [InlineData(6,5)] [InlineData(20,5)] [InlineData(21,10)] [InlineData(50,10)] [InlineData(51,15)]
    public void Limites_de_descuento_por_unidades(int unidades,decimal descuento) => Assert.Equal(descuento,CalculoPedido.Calcular(100,unidades,false,12).DescuentoVolumen);
    [Theory]
    [InlineData(false,324.52)] [InlineData(true,298.56)]
    public void Ejemplos_academicos_y_orden_de_descuentos(bool institucional,decimal total) => Assert.Equal(total,CalculoPedido.Calcular(305,15,institucional,12).TotalFinal);
    private static ActorCompra Cliente(string id,bool institucional=false) => new(id,true,institucional,false);
    private static async Task<(int Fisico,int Digital)> Productos(InstitucionalEscenario es) => await es.EnScope(async sp=>
    {
        var db=sp.GetRequiredService<EditorialDbContext>();var editorial=new tblEditorial { NombreEditorial="Prueba compras "+Guid.NewGuid() };
        var libro=new tblLibro { Titulo="Libro de prueba",Sinopsis="Prueba temporal",AnioEdicion=2026,portadaURL="/images/sin-portada.svg",fechaRegistro=DateTime.Now,Activo=true,IDEditorialNavigation=editorial };
        var f=new tblPresentacionLibro { IDLibroNavigation=libro,IDFormatoNavigation=new(){NombreFormato="Impreso "+Guid.NewGuid()},isbn=Guid.NewGuid().ToString("N")[..20],PrecioBase=25.50m,Existencias=100 };
        var d=new tblPresentacionLibro { IDLibroNavigation=libro,IDFormatoNavigation=new(){NombreFormato="Digital "+Guid.NewGuid(),EsDigital=true},isbn=Guid.NewGuid().ToString("N")[..20],PrecioBase=10,Existencias=null };
        db.AddRange(f,d);await db.SaveChangesAsync();return(f.IDPresentacion,d.IDPresentacion);
    });
    [Fact]
    public async Task Compra_transaccional_carrito_persistido_stock_historial_y_reenvio()
    {
        await using var es=await CrearAsync();await es.ConfigurarImpuestoAsync(12);var usuario=await es.UsuarioAsync("ClienteMinorista");var productos=await Productos(es);
        var pedido=await es.EnScope(async sp=>
        {
            var srv=sp.GetRequiredService<CarritoService>();var actor=Cliente(usuario);
            await srv.Agregar(actor,new(){PresentacionId=productos.Fisico,Cantidad=10});await srv.Agregar(actor,new(){PresentacionId=productos.Digital,Cantidad=5});
            var carrito=await srv.Ver(actor);Assert.Equal(324.52m,carrito.Totales!.TotalFinal);
            var entrada=new ConfirmarCompra { Confirmacion=carrito.Confirmacion!,Direccion="Dirección de prueba" };
            int id=await srv.Confirmar(actor,entrada);
            Assert.Empty((await srv.Ver(actor)).Lineas);
            await Assert.ThrowsAsync<ReglaCompraException>(()=>srv.Confirmar(actor,entrada));
            var db=sp.GetRequiredService<EditorialDbContext>();Assert.Equal(90,(await db.tblPresentacionLibros.FindAsync(productos.Fisico))!.Existencias);Assert.Null((await db.tblPresentacionLibros.FindAsync(productos.Digital))!.Existencias);
            var detalle=await sp.GetRequiredService<PedidosService>().Detalle(actor,id);Assert.Equal(2,detalle.Lineas.Count);Assert.Equal(324.52m,detalle.Totales.TotalFinal);return id;
        });
        using var cliente=es.Cliente(usuario,"ClienteMinorista");var html=await cliente.GetStringAsync($"/Pedidos/Detalles/{pedido}");Assert.Contains("324.52",html);Assert.Contains("Dirección",WebUtility.HtmlDecode(html));
        using var anonimo=es.Cliente();Assert.Equal(HttpStatusCode.Unauthorized,(await anonimo.GetAsync("/Carrito")).StatusCode);
        var otro=await es.UsuarioAsync("ClienteMinorista");using var ajeno=es.Cliente(otro,"ClienteMinorista");Assert.Equal(HttpStatusCode.NotFound,(await ajeno.GetAsync($"/Pedidos/Detalles/{pedido}")).StatusCode);
        await es.EnScope(async sp=>
        {
            var pedidos=sp.GetRequiredService<PedidosService>();await Assert.ThrowsAsync<CompraNoEncontradaException>(()=>pedidos.Detalle(Cliente(otro),pedido));
            await Assert.ThrowsAsync<AccesoCompraException>(()=>pedidos.CambiarEstado(Cliente(usuario),pedido,"Entregado"));
            var admin=new ActorCompra("administrador",false,false,true);await Assert.ThrowsAsync<ReglaCompraException>(()=>pedidos.CambiarEstado(admin,pedido,"Entregado"));
            await pedidos.CambiarEstado(admin,pedido,"EnProceso");await pedidos.CambiarEstado(admin,pedido,"Enviado");await pedidos.CambiarEstado(admin,pedido,"Entregado");return true;
        });
    }
    [Fact]
    public async Task Carrito_propiedad_cantidades_cambios_de_precio_y_falta_de_stock()
    {
        await using var es=await CrearAsync();var usuario=await es.UsuarioAsync("ClienteMinorista");var otro=await es.UsuarioAsync("ClienteMinorista");var producto=await Productos(es);
        await es.EnScope(async sp=>
        {
            var srv=sp.GetRequiredService<CarritoService>();var actor=Cliente(usuario);var db=sp.GetRequiredService<EditorialDbContext>();
            await Assert.ThrowsAsync<ReglaCompraException>(()=>srv.Agregar(actor,new(){PresentacionId=producto.Fisico,Cantidad=0}));
            await srv.Agregar(actor,new(){PresentacionId=producto.Fisico,Cantidad=2});var vista=await srv.Ver(actor);var linea=Assert.Single(vista.Lineas);
            await Assert.ThrowsAsync<CompraNoEncontradaException>(()=>srv.Quitar(Cliente(otro),linea.Id));
            await Assert.ThrowsAsync<ReglaCompraException>(()=>srv.Cambiar(actor,linea.Id,-1));
            await srv.Cambiar(actor,linea.Id,3);
            await Assert.ThrowsAsync<ReglaCompraException>(()=>srv.Confirmar(actor,new(){Confirmacion=vista.Confirmacion!,Direccion="Prueba"}));
            vista=await srv.Ver(actor);var p=(await db.tblPresentacionLibros.FindAsync(producto.Fisico))!;p.PrecioBase=30;await db.SaveChangesAsync();
            await Assert.ThrowsAsync<ReglaCompraException>(()=>srv.Confirmar(actor,new(){Confirmacion=vista.Confirmacion!,Direccion="Prueba"}));
            vista=await srv.Ver(actor);p.Existencias=2;await db.SaveChangesAsync();
            await Assert.ThrowsAsync<ReglaCompraException>(()=>srv.Confirmar(actor,new(){Confirmacion=vista.Confirmacion!,Direccion="Prueba"}));Assert.Equal(2,p.Existencias);Assert.Empty(await db.tblPedidos.Where(p=>p.IDUsuario==usuario).ToListAsync());
            await srv.Vaciar(actor);Assert.Empty((await srv.Ver(actor)).Lineas);return true;
        });
    }
    [Fact]
    public async Task Descuento_institucional_exige_perfil_aprobado_y_digital_no_exige_direccion()
    {
        await using var es=await CrearAsync();await es.ConfigurarImpuestoAsync(12);var usuario=await es.UsuarioAsync("ClienteInstitucional");var productos=await Productos(es);
        await es.EnScope(async sp=>
        {
            var srv=sp.GetRequiredService<CarritoService>();var actor=Cliente(usuario,true);var db=sp.GetRequiredService<EditorialDbContext>();
            await srv.Agregar(actor,new(){PresentacionId=productos.Digital,Cantidad=15});Assert.False((await srv.Ver(actor)).Institucional);
            db.Add(new tblPerfilInstitucional { IDUsuario=usuario,NombreInstitucion="Instituto temporal",NIT="Prueba",Direccion="Prueba",EstadoValidacion="Aprobado" });await db.SaveChangesAsync();
            var vista=await srv.Ver(actor);Assert.True(vista.Institucional);Assert.Equal(11.40m,vista.Totales!.DescuentoInstitucional);
            var id=await srv.Confirmar(actor,new(){Confirmacion=vista.Confirmacion!});var pedido=await sp.GetRequiredService<PedidosService>().Detalle(actor,id);Assert.Null(pedido.Direccion);
            return true;
        });
    }
    [Fact]
    public async Task Formularios_HTTP_protegidos_por_rol_y_antifalsificacion()
    {
        await using var es=await CrearAsync();var usuario=await es.UsuarioAsync("ClienteMinorista");var productos=await Productos(es);using var cliente=es.Cliente(usuario,"ClienteMinorista");
        var html=await cliente.GetStringAsync($"/Catalogo/Detalles/{await es.EnScope(async sp=>(await sp.GetRequiredService<EditorialDbContext>().tblPresentacionLibros.FindAsync(productos.Fisico))!.IDLibro)}");
        var token=Token(html);
        Assert.Equal(HttpStatusCode.BadRequest,(await cliente.PostAsync("/Carrito/Agregar",new FormUrlEncodedContent([]))).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect,(await cliente.PostAsync("/Carrito/Agregar",Form(token,("PresentacionId",productos.Fisico.ToString()),("Cantidad","2"),("Precio","0.01")))).StatusCode);
        var carrito=await cliente.GetStringAsync("/Carrito");Assert.Contains("Confirmar compra",carrito);Assert.Contains("51.00",carrito);
        using var asesor=es.Cliente(await es.UsuarioAsync("AsesorVentas"),"AsesorVentas");Assert.Equal(HttpStatusCode.Forbidden,(await asesor.GetAsync("/Carrito")).StatusCode);
    }
}
