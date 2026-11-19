# Portadas locales asociadas — 19/11/2026

## Resultado

**20 portadas JPG asociadas; 0 faltantes; 0 imágenes sin correspondencia por nombre de archivo.** Se utilizaron exclusivamente los archivos proporcionados por el usuario en `PlataformaEditorial.Web/wwwroot/images/portadas/`. No se consultó Internet ni se descargaron imágenes. No hubo despliegues ni push a GitHub.

Los 20 archivos se abrieron y decodificaron como JPEG con System.Drawing de Windows, conservando nombres, extensiones, dimensiones y bytes originales. Se comprobaron los ISBN exactos de los nombres contra las fuentes locales, los scripts y el catálogo. `PORTADAS-LIBROS.csv` registra título, ISBN, ruta, tamaño y SHA-256 de cada archivo. La fuente de las imágenes es el aporte local del usuario; los enlaces bibliográficos del CSV son los ya existentes, no nuevas consultas ni evidencia de licencia.

Se modificó exclusivamente `tblLibro.portadaURL` para los 20 libros. La primera ejecución actualizó 20 rutas; repetirla actualizó 0. No se cambiaron títulos, ISBN, precios, inventario, controladores, servicios, vistas, autenticación ni otras funcionalidades. Los tres libros ficticios preexistentes conservan la imagen predeterminada y quedan fuera de estos 20.

**Alcance de la correspondencia:** se asocia por el ISBN del nombre del archivo, tal como se solicitó. Eso no certifica la edición impresa representada en la imagen. En la revisión visual, `9788445006771.jpg` (Fahrenheit 451) muestra texto en inglés y “60th Anniversary Edition”; conviene revisar manualmente si representa la edición española registrada. Se conservó y se asoció el archivo entregado, sin sustituirlo ni cambiar el ISBN. No faltan archivos para completar las asociaciones técnicas.

## Inventario

Todas las rutas usan `/images/portadas/{nombre exacto}`. La misma portada del libro aparece en sus presentaciones impresas y digitales, porque el esquema guarda la ruta en el libro, no en cada presentación.

| Título | ISBN impreso | Archivo en `wwwroot/images/portadas/` | Resultado |
|---|---|---|---|
| Cien años de soledad | `9788497592208` | `9788497592208.jpg` | Asociada, HTTP 200 |
| Fahrenheit 451 | `9788445006771` | `9788445006771.jpg` | Asociada, HTTP 200 |
| Crónicas marcianas | `9788445006740` | `9788445006740.jpg` | Asociada, HTTP 200 |
| El hombre ilustrado | `9788445006801` | `9788445006801.jpg` | Asociada, HTTP 200 |
| Soy leyenda | `9788445006764` | `9788445006764.jpg` | Asociada, HTTP 200 |
| El hobbit | `9788445012802` | `9788445012802.jpg` | Asociada, HTTP 200 |
| La Comunidad del Anillo | `9788445013557` | `9788445013557.jpg` | Asociada, HTTP 200 |
| Las Dos Torres | `9788445013564` | `9788445013564.jpg` | Asociada, HTTP 200 |
| El Silmarillion | `9788445014493` | `9788445014493.jpg` | Asociada, HTTP 200 |
| El fin de la eternidad | `9788497933537` | `9788497933537.jpg` | Asociada, HTTP 200 |
| La metamorfosis | `9788420651361` | `9788420651361.jpg` | Asociada, HTTP 200 |
| El retrato de Dorian Gray | `9788420654935` | `9788420654935.jpg` | Asociada, HTTP 200 |
| Drácula | `9788491819103` | `9788491819103.jpg` | Asociada, HTTP 200 |
| La Isla del Tesoro | `9788420651385` | `9788420651385.jpg` | Asociada, HTTP 200 |
| Frankenstein | `9788413623689` | `9788413623689.jpg` | Asociada, HTTP 200 |
| Orgullo y prejuicio | `9791370093068` | `9791370093068.jpg` | Asociada, HTTP 200 |
| Alicia en el País de las Maravillas | `9788420664286` | `9788420664286.jpg` | Asociada, HTTP 200 |
| Mujercitas | `9788413628448` | `9788413628448.jpg` | Asociada, HTTP 200 |
| Odisea | `9788420674629` | `9788420674629.jpg` | Asociada, HTTP 200 |
| Ilíada | `9788420678344` | `9788420678344.jpg` | Asociada, HTTP 200 |

## Reproducir en otra computadora

Incluir la carpeta completa `PlataformaEditorial.Web/wwwroot/` al comprimir el proyecto, junto con scripts y documentación. Preparar SQL Server/Identity/tablas existentes según README; no recrear una base que ya contiene datos.

Para una instalación nueva, después de crear el esquema, ejecutar:

```powershell
sqlcmd -S '.\SQLEXPRESS' -d EditorialDB -E -C -b -f 65001 -i scripts/libros-reales.sql
```

El script inicial incluye las 20 rutas explícitas, las usa al insertar libros y actualiza solo `portadaURL` de los libros existentes identificados por ISBN. Conserva el comportamiento anterior de insertar datos bibliográficos faltantes, sin reponer existencias ni cambiar precios existentes.

Si los 20 libros ya están cargados y solo se desean asociar las imágenes:

```powershell
sqlcmd -S '.\SQLEXPRESS' -d EditorialDB -E -C -b -f 65001 -i scripts/asociar-portadas-locales.sql
```

El segundo script valida los 20 ISBN/títulos antes de actualizar, utiliza transacción y se puede repetir. Sustituye la ruta anterior de esos 20 libros por la ruta local explícita; no modifica otros registros. SQL no inspecciona el disco: llevar también las imágenes y ejecutar la comprobación de archivos. No cargar textos acentuados por tuberías; usar los archivos UTF-8 con `-f 65001 -i`.

## Verificaciones

- Compilación: `dotnet build PlataformaEditorial.slnx --no-restore`, 0 errores y 0 advertencias.
- Suite existente: `dotnet test PlataformaEditorial.slnx --no-build --no-restore`, 39 correctas, 0 fallidas y 0 omitidas.
- Script inicial ejecutado sobre los datos existentes dentro de una transacción exterior revertida: correcto, con las 20 rutas. No se realizó una instalación completa en una segunda computadora.
- Asociación SQL ejecutada dos veces: 20 y 0 actualizaciones respectivamente.
- Comprobación automatizada: 20 archivos con hashes originales, rutas coincidentes en CSV y ambos scripts SQL; 43 páginas HTTP 200 (inicio, dos páginas de catálogo, 20 búsquedas por ISBN y 20 detalles), 74 referencias locales. Cada una de las 20 imágenes se sirvió con bytes idénticos al archivo local.
- Revisión visual en navegador de inicio, catálogo, búsqueda y detalles: las imágenes se muestran sin iconos rotos. La revisión visual fue representativa; el recorrido HTTP cubrió los 20 libros.
- No se desconectó la red de Windows: se verificó que las referencias de portada son locales y que sus archivos están incluidos. Restaurar NuGet en otra máquina puede requerir Internet inicialmente; las portadas no.

Prueba reproducible de archivos, hashes y coherencia de scripts (Python 3.10+, sin paquetes externos):

```powershell
python scripts/verificar-portadas.py
```

Para incluir las comprobaciones HTTP, ejecutar primero la aplicación y después el verificador en otra terminal:

```powershell
dotnet run --project PlataformaEditorial.Web --launch-profile http
python scripts/verificar-portadas.py --base-url http://localhost:5172
```

## Git y entrega

Las imágenes JPG no están excluidas por `.gitignore`. Se incorporan las 20 imágenes originales a Git en `feature/carrito-pedidos`. Se verifica un ZIP temporal generado desde Git: contiene las 20 rutas y sus hashes originales. No se incluye una base de datos SQL dentro del ZIP del código ni se publica el repositorio remoto.

Archivos modificados: `scripts/libros-reales.sql`, `scripts/asociar-portadas-locales.sql`, `docs/PORTADAS-LIBROS.csv`, este informe, `docs/PORTADAS-Y-FUENTES.md` y `README.md`. Archivos incorporados: las 20 imágenes JPG y `scripts/verificar-portadas.py`. No se modificó la estructura de la base de datos. El mecanismo MVC se conserva: SQL guarda la ruta, el servicio la entrega y las vistas la muestran.
