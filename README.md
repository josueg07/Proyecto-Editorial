# Plataforma Editorial MVC

Proyecto académico de Arquitectura de Sistemas, preparado para la entrega del 13 de noviembre de 2026. Solución existente: `PlataformaEditorial.slnx`; aplicación: `PlataformaEditorial.Web`. Rama de entrega: **`feature/carrito-pedidos`**, sin integración automática a `main`.

## Qué permite hacer

- Consultar catálogo, destacados, novedades, búsqueda y filtros combinados.
- Administrar libros, autores, categorías, editoriales, formatos y presentaciones.
- Registrar usuarios y mantener cuentas con ASP.NET Core Identity.
- Comprar presentaciones impresas o digitales mediante carrito persistente, descuentos, IVA, pedidos e inventario transaccional.
- Consultar el historial propio y supervisar los pedidos como administrador.
- Solicitar y aprobar perfiles institucionales, asignar solicitudes a asesores y emitir, responder, imprimir o descargar cotizaciones PDF (RF-04).
- Navegar por las opciones correspondientes al rol y consultar el estado del perfil institucional.

La interfaz, los errores habituales y las validaciones están en español, con cultura `es-GT`. No se incluyen pagos electrónicos ni archivos de libros para descarga automática.

## Requisitos

- Windows con Visual Studio 2026 y carga de trabajo de desarrollo web, o SDK de .NET 10.
- SQL Server Express accesible en `.\SQLEXPRESS`; base `EditorialDB`.
- Acceso de la cuenta Windows al servidor SQL. La conexión de ejemplo usa autenticación integrada y no contiene contraseña.
- `sqlcmd` (o SQL Server Management Studio) para ejecutar los scripts.
- Acceso a NuGet para restaurar EF Core 10, Identity y QuestPDF 2026.9.2. QuestPDF se configura con licencia Community para este proyecto académico; consultar sus condiciones antes de reutilizarlo en otro contexto.

## Abrir el proyecto existente

```powershell
git clone --branch feature/carrito-pedidos https://github.com/josueg07/Proyecto-Editorial.git
cd Proyecto-Editorial
dotnet tool restore
dotnet restore PlataformaEditorial.slnx
```

Si ya existe una copia local, utilizarla y seleccionar la rama de entrega; no crear otra solución. Para cambiar servidor o conexión sin publicar credenciales, usar Secret Manager o variables de entorno:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.\SQLEXPRESS;Database=EditorialDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true" --project PlataformaEditorial.Web
```

Para ejecutar los comandos fuera del entorno Development también puede establecerse `ConnectionStrings__DefaultConnection` en el entorno local. La herramienta de usuarios admite asimismo `EDITORIAL_CONNECTION`. No guardar contraseñas en Git ni en argumentos de la aplicación.

## Base de datos: entorno existente

Si `EditorialDB` ya contiene Identity y las 19 tablas de negocio, **no recrearla ni volver a ejecutar la migración inicial a ciegas**. La aplicación no ejecuta migraciones automáticamente. Revisar la conexión y cargar solamente los datos faltantes mediante los scripts idempotentes que correspondan.

El contexto `ApplicationDbContext` administra únicamente Identity. `EditorialDbContext` representa las tablas del negocio mediante Database First; no se deben generar ni aplicar migraciones de negocio sobre ellas.

## Instalación en otra computadora, con base nueva

Estos pasos son para una base nueva o una copia preparada para instalación, no para reemplazar el entorno existente:

1. Instalar/iniciar SQL Server Express y configurar la conexión a `EditorialDB`.
2. Aplicar la migración existente de **Identity**. EF puede crear la base si la cuenta tiene permisos. La fábrica de diseño evita ejecutar la creación de roles antes de que existan sus tablas.
3. Crear las tablas del negocio con el script que conserva el esquema inspeccionado.
4. Cargar el IVA académico autorizado y los libros verificables.

```powershell
dotnet ef database update --context ApplicationDbContext --project PlataformaEditorial.Web
sqlcmd -S '.\SQLEXPRESS' -d EditorialDB -E -C -b -f 65001 -i scripts/estructura-negocio.sql
sqlcmd -S '.\SQLEXPRESS' -d EditorialDB -E -C -b -f 65001 -i scripts/configurar-iva-academico.sql
sqlcmd -S '.\SQLEXPRESS' -d EditorialDB -E -C -b -f 65001 -i scripts/libros-reales.sql
```

La migración de Identity solo crea sus tablas y su historial. El script del negocio no crea Identity, no contiene eliminaciones y no modifica columnas existentes. Fue generado desde los nombres y tipos reales y se comprobó su sintaxis; no se ensayó una instalación completa en una segunda instancia de SQL Server durante esta entrega. Si una base existente tiene tablas de Identity pero carece de historial de migraciones, revisar esa situación antes de aplicar la migración inicial.

No ejecutar `scripts/datos-demo.sql` para la entrega final: corresponde a los tres libros ficticios del trabajo inicial. Los scripts nuevos no eliminan esos registros si todavía existen.

## Compilar y ejecutar

```powershell
dotnet build PlataformaEditorial.slnx --no-restore
dotnet test PlataformaEditorial.slnx --no-restore
dotnet run --project PlataformaEditorial.Web --launch-profile http
```

Abrir `http://localhost:5172`. En Visual Studio, establecer `PlataformaEditorial.Web` como proyecto de inicio y elegir el perfil `http` o `https`. Detener cualquier instancia previa en el mismo puerto antes de iniciar otra.

## Usuarios de demostración y correo

**No hay un servicio real de correo configurado.** La aplicación no anuncia envíos inexistentes y no publica enlaces/tokens de confirmación. Se mantiene la confirmación obligatoria para iniciar sesión. Recuperación por correo, reenvío y cambio de correo indican la limitación; no se simulan envíos.

El responsable del entorno puede crear cuentas o confirmar las registradas mediante la herramienta local, que utiliza `UserManager<IdentityUser>`, tokens de Identity y la misma base. No debe exponerse como servicio público.

```powershell
dotnet run --project tools/PlataformaEditorial.Herramientas -- crear administrador@ejemplo.invalid Administrador
dotnet run --project tools/PlataformaEditorial.Herramientas -- crear asesor@ejemplo.invalid AsesorVentas
dotnet run --project tools/PlataformaEditorial.Herramientas -- crear cliente@ejemplo.invalid ClienteMinorista
dotnet run --project tools/PlataformaEditorial.Herramientas -- confirmar cuenta-registrada@ejemplo.invalid
```

Los correos anteriores son ejemplos; elegir cuentas de demostración propias. `crear` solicita la contraseña de forma oculta en una terminal interactiva y valida las reglas de Identity. No sobrescribe una cuenta existente. No se incluyen contraseñas predeterminadas, personales ni secretos en el repositorio.

Para un cliente institucional, crear/registrar primero un minorista, confirmar la cuenta, enviar el perfil desde el navegador y aprobarlo con el administrador. **La herramienta no asigna `ClienteInstitucional`**: ese rol solo se concede desde la aprobación de RF-04. El cliente debe cerrar sesión y volver a entrar después del cambio de rol.

## Recorrido de demostración

1. Visitante: Inicio → Catálogo → buscar por autor/ISBN y combinar categoría, precio y formato → Detalle del libro.
2. Minorista confirmado: agregar dos presentaciones, abrir Carrito, cambiar cantidades, quitar una línea y volver a agregarla. Revisar subtotal, descuentos e IVA. Para libros impresos, indicar dirección y confirmar.
3. Comprobar que el carrito queda vacío y el pedido aparece en Mis pedidos. Recargar/repetir el formulario anterior no debe generar otro pedido.
4. Comparar existencias impresas antes/después. Una presentación digital puede comprarse sin existencias físicas ni dirección; no se descuenta stock físico.
5. Administrador: Administración → Gestionar pedidos → pasar por los estados válidos. Impreso: Pendiente → En proceso → Enviado → Entregado. Solo digital: Pendiente → En proceso → Entregado.
6. Minorista: Instituciones → Solicitar perfil institucional. Tras enviarlo, el menú muestra el estado pendiente y no invita a crear otro perfil.
7. Administrador: validar el perfil. Cliente: cerrar/abrir sesión y repetir una compra para comprobar el 8 % adicional.
8. RF-04: nueva solicitud → asignación al asesor → borrador → emisión → revisión/PDF → aprobación o rechazo por la institución propietaria.
9. Probar las URL de un pedido/solicitud con otra cuenta: no debe poder consultarlos ni modificarlos.
10. Probar cantidades negativas, stock insuficiente, formulario de confirmación anterior a un cambio de precio y rutas inexistentes; los mensajes deben estar en español.

## Cálculos de compra

Se reutiliza **sin modificar** `Models/Servicios/Compras/CalculoPedido.cs`:

| Unidades totales del pedido | Descuento por volumen |
| --- | --- |
| 1–5 | 0 % |
| 6–20 | 5 % |
| 21–50 | 10 % |
| Desde 51 | 15 % |

Después se aplica el 8 % sobre la base ya descontada, únicamente cuando hay rol institucional y perfil aprobado. El IVA se consulta en `tblConfiguracionSistema` y se aplica después de ambos descuentos. El redondeo monetario es a dos decimales, `AwayFromZero`.

Ejemplo: subtotal Q305 y 15 unidades. Volumen: Q15.25. Minorista: IVA Q34.77, **total Q324.52**. Institucional aprobado: descuento adicional Q23.18, IVA Q31.99, **total Q298.56**.

## Arquitectura MVC

- **Modelo:** `Models/Entidades` y `Data/Negocio` conservan Database First. Los servicios realizan consultas, comprueban propiedad/roles, aplican reglas y coordinan transacciones. Los modelos específicos de formulario impiden recibir precios o totales como datos autoritativos del navegador.
- **Controladores:** coordinan HTTP, `ModelState`, servicios, redirecciones y respuestas. Las escrituras son POST con antifalsificación y autorización.
- **Vistas:** Razor/Bootstrap muestran los modelos y formularios. No consultan directamente la base ni calculan reglas comerciales. El menú institucional se prepara mediante un ViewComponent.
- **Identity:** usa `IdentityUser` y `ApplicationDbContext` separados del negocio. El registro público conserva exclusivamente el rol minorista.

## Datos bibliográficos y límites

`scripts/libros-reales.sql` contiene 20 libros reales, 20 ISBN impresos y tres ISBN digitales. Se ejecutó dos veces en el entorno local y se verificó la idempotencia. Los precios e inventarios son académicos, no ofertas reales. Cada edición impresa nueva comienza con 120 ejemplares; repetir el script no repone stock ni modifica precios.

Las fuentes y las instrucciones para portadas autorizadas están en [docs/PORTADAS-Y-FUENTES.md](docs/PORTADAS-Y-FUENTES.md) y [docs/FUENTES-LIBROS.csv](docs/FUENTES-LIBROS.csv). Las 20 imágenes aportadas por el usuario están guardadas localmente en `wwwroot/images/portadas/` y asociadas por ISBN; no se descargaron imágenes para esta actualización.

La [revisión de portadas del 19/11/2026](docs/REVISION-PORTADAS.md) documenta los 20 títulos, ISBN, archivos y resultados. Los 20 tienen portada local. `scripts/asociar-portadas-locales.sql` actualiza exclusivamente sus rutas de portada, incluso si ya existía otra ruta; el script inicial incluye las mismas asociaciones. Incluir toda la carpeta `wwwroot` en el ZIP. La comprobación reproducible está en `scripts/verificar-portadas.py`.

Las compras registran pedidos reales en SQL, pero no cobran pagos electrónicos. La entrega digital se coordina manualmente porque el esquema no almacena archivos ni licencias de distribución. No se implementan cancelaciones/devoluciones ni reposición automática de inventario; los estados avanzan en una dirección. El historial conserva cantidades, precios, descuentos e IVA del pedido, mientras el título/formato se obtiene de la presentación relacionada porque no existen columnas de descripción histórica.

## Verificación y documentación adicional

La suite verificada contiene **39 pruebas**, incluidas regresiones de catálogo y RF-04, descuentos en 5/6/20/21/50/51 unidades, ejemplos monetarios, carrito, inventario, pedidos, permisos, antifalsificación, cambios de precio, confirmación repetida y localización.

Las pruebas SQL usan transacciones revertidas. Configurar `EDITORIAL_TEST_CONNECTION` para apuntarlas a una copia compatible si se desea. Los IDs pueden avanzar aunque las filas se reviertan. Las pruebas se ejecutan en serie para evitar interferencias entre transacciones de distintos escenarios sobre la misma base.

- [Revisión final y archivos](docs/REVISION-FINAL.md)
- [Catálogo y arquitectura inicial](docs/CATALOGO-MVC.md)
- [Flujo institucional RF-04](docs/RF04-INSTITUCIONAL.md)

La guía RF-04 describe el estado anterior al cierre: la navegación compartida y el tratamiento del correo se actualizaron en esta entrega; este README y la revisión final prevalecen en esos puntos.
