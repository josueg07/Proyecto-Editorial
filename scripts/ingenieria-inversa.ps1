# Ejecutar desde la raíz de la solución. Solo lee el esquema; no aplica migraciones.
$ErrorActionPreference = 'Stop'
$tablas = @(
    'tblAutor','tblBanner','tblCarrito','tblCategoria','tblConfiguracionSistema',
    'tblCotizacion','tblDetalleCarrito','tblDetalleCotizacion','tblDetallePedido',
    'tblDetalleSolicitudCotizacion','tblEditorial','tblFormatoLibro','tblLibro',
    'tblLibro_Autor','tblLibro_Categoria','tblPedido','tblPerfilInstitucional',
    'tblPresentacionLibro','tblSolicitudCotizacion'
)
dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw 'No se pudo restaurar dotnet-ef.' }
$argumentos = @('ef','dbcontext','scaffold','Name=ConnectionStrings:DefaultConnection',
    'Microsoft.EntityFrameworkCore.SqlServer','--project','PlataformaEditorial.Web',
    '--context','EditorialDbContext','--context-dir','Data/Negocio',
    '--output-dir','Models/Entidades','--no-onconfiguring','--use-database-names')
foreach ($tabla in $tablas) { $argumentos += @('--table',$tabla) }
# Intencionalmente no se agrega --force.
dotnet @argumentos
if ($LASTEXITCODE -ne 0) { throw 'Scaffold no completado. Revise la salida; no sobrescriba archivos sin compararlos.' }
