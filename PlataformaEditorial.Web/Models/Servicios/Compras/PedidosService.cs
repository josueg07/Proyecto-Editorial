using Microsoft.EntityFrameworkCore;
using PlataformaEditorial.Web.Data.Negocio;
using PlataformaEditorial.Web.Models.Compras;
namespace PlataformaEditorial.Web.Models.Servicios.Compras;
public class PedidosService(EditorialDbContext db,UnidadCompra unidad)
{
    private IQueryable<PlataformaEditorial.Web.Models.Entidades.tblPedido> Consulta(ActorCompra actor)
    {
        if(!actor.Administrador && !actor.Cliente) throw new AccesoCompraException();
        return actor.Administrador ? db.tblPedidos : db.tblPedidos.Where(p=>p.IDUsuario==actor.Id);
    }
    public Task<List<PedidoFila>> Listar(ActorCompra actor) => Consulta(actor).AsNoTracking().OrderByDescending(p=>p.IDPedido).Select(p=>new PedidoFila(p.IDPedido,p.FechaPedido,p.EstadoPedido,p.TotalFinal)).ToListAsync();
    public async Task<PedidoVista> Detalle(ActorCompra actor,int id)
    {
        var p=await Consulta(actor).AsNoTracking().Include(p=>p.tblDetallePedidos).ThenInclude(d=>d.IDPresentacionNavigation).ThenInclude(p=>p.IDLibroNavigation)
            .Include(p=>p.tblDetallePedidos).ThenInclude(d=>d.IDPresentacionNavigation).ThenInclude(p=>p.IDFormatoNavigation).SingleOrDefaultAsync(p=>p.IDPedido==id) ?? throw new CompraNoEncontradaException();
        var lineas=p.tblDetallePedidos.Select(d=>new LineaCarrito(d.IDDetallePedido,d.IDPresentacion,d.IDPresentacionNavigation.IDLibroNavigation.Titulo,d.IDPresentacionNavigation.IDFormatoNavigation.NombreFormato,d.IDPresentacionNavigation.isbn,d.Cantidad,d.PrecioUnitario,d.IDPresentacionNavigation.IDFormatoNavigation.EsDigital,null,true)).ToList();
        return new(p.IDPedido,p.FechaPedido,p.EstadoPedido,p.DireccionEntrega,lineas,new(p.SubtotalBase,p.DescuentoVolumen,p.DescuentoInstitucional,p.Impuestos,p.TotalFinal),p.PorcentajeImpuesto,actor.Administrador?Siguientes(p.EstadoPedido,lineas.All(l=>l.Digital)):[]);
    }
    private static List<string> Siguientes(string estado,bool digital) => estado switch { "Pendiente"=>["EnProceso"],"EnProceso"=>digital?["Entregado"]:["Enviado"],"Enviado"=>["Entregado"],_=>[] };
    public Task CambiarEstado(ActorCompra actor,int id,string estado) => unidad.Ejecutar(async ()=>
    {
        if(!actor.Administrador) throw new AccesoCompraException();
        var pedido=await db.tblPedidos.FromSqlInterpolated($"SELECT * FROM dbo.tblPedido WITH (UPDLOCK,HOLDLOCK) WHERE IDPedido={id}").SingleOrDefaultAsync() ?? throw new CompraNoEncontradaException();
        bool digital=await db.tblDetallePedidos.Where(d=>d.IDPedido==id).AllAsync(d=>d.IDPresentacionNavigation.IDFormatoNavigation.EsDigital);
        if(!Siguientes(pedido.EstadoPedido,digital).Contains(estado)) throw new ReglaCompraException("La transición de estado no está permitida.");
        pedido.EstadoPedido=estado;await db.SaveChangesAsync();return true;
    });
}
