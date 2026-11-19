# Revisión de portadas locales — 19/11/2026

## Resultado y alcance

**0 portadas editoriales descargadas; 20 pendientes de autorización verificable para incorporarlas al repositorio y al ZIP.** Los 20 títulos sí tienen una imagen predeterminada local válida. Esto permite mostrar el catálogo sin Internet, pero no equivale a completar las portadas de las ediciones. No se usaron portadas de otras ediciones, ni se cambiaron ISBN, ni se generaron imitaciones.

Se inspeccionaron `EditorialDbContext`, `tblLibro.portadaURL`, `tblPresentacionLibro.isbn`, el servicio y las vistas del catálogo y `scripts/libros-reales.sql`. La base existente contiene los 20 títulos de las fuentes bibliográficas más tres libros de demostración ajenos a esta revisión. Los 20 tenían `/images/sin-portada.svg`; no había JPG/WebP ni asociaciones de portadas correctas que reemplazar. No se eliminaron registros.

La imagen compartida está en `PlataformaEditorial.Web/wwwroot/images/sin-portada.svg` (300 × 420, vectorial, 435 bytes). Es el recurso del proyecto, sin imágenes editoriales. No se crean 20 copias innecesarias del mismo marcador. La carpeta `wwwroot/images/portadas/` queda reservada en Git para imágenes autorizadas futuras.

## Fuentes y condiciones consultadas

Se consultaron páginas y condiciones oficiales, sin scraping ni descargas masivas. Las fichas bibliográficas son las registradas previamente en `FUENTES-LIBROS.csv`; no se afirma haber cotejado visualmente una imagen que no se descargó. El ISBN y el título de las 20 filas sí se cotejaron contra SQL Server y el script inicial.

- **P — Minotauro / Planeta:** [Condiciones de uso, apartado 7](https://www.planetadelibros.com/condiciones-de-uso). No se encontró una licencia de reproducción y redistribución de las portadas aplicable a esta entrega.
- **R — Debolsillo / Penguin:** [Aviso legal, propiedad intelectual y reserva de derechos](https://www.penguinlibros.com/es/content/2-aviso-legal). Contempla acceso y, en su caso, descarga personal no comercial; no concede una licencia para incluir las imágenes en el repositorio o redistribuir el ZIP.
- **A — Alianza Editorial:** [Aviso legal, uso de la información](https://www.alianzaeditorial.es/aviso-legal/). Permite visualizar, descargar e imprimir material con condiciones, conservando avisos. No se encontró autorización expresa para redistribuir esas imágenes dentro de esta entrega.

[Open Library Covers API](https://openlibrary.org/dev/docs/api/covers) ofrece acceso técnico a imágenes y pide enlazar sus servidores para páginas públicas; esa documentación no acredita una licencia de redistribución para cada portada. No se utilizó como sustituto de un permiso del titular. La autorización para recursos promocionales estadounidenses de Penguin no se extendió a estas ediciones españolas.

Esta revisión registra los permisos encontrados, sin afirmar que un uso académico sea necesariamente ilícito ni que estas obras carezcan de posibles excepciones legales. Se aplicó la condición del encargo: descargar únicamente imágenes con autorización verificable y conservar la predeterminada en los demás casos. Que el texto de una obra clásica sea de dominio público no demuestra que la cubierta de una edición moderna también lo sea.

## Inventario de los 20 títulos

Todas las rutas actuales son `/images/sin-portada.svg`. La fuente de cada fila identifica la edición, no una licencia de su imagen. El CSV `PORTADAS-LIBROS.csv` contiene las rutas completas, los enlaces a las condiciones y el estado por libro.

| Título | ISBN impreso | Archivo local | Fuente / permiso | Resultado |
|---|---|---|---|---|
| Cien años de soledad | `9788497592208` | `sin-portada.svg` | [Ficha](https://www.penguinlibros.com/es/literatura-contemporanea/35974-libro-cien-anos-de-soledad-9788497592208) / R | Pendiente; predeterminada local |
| Fahrenheit 451 | `9788445006771` | `sin-portada.svg` | [Ficha](https://www.planetadelibros.com/libro-fahrenheit-451/298205) / P | Pendiente; predeterminada local |
| Crónicas marcianas | `9788445006740` | `sin-portada.svg` | [Ficha](https://www.planetadelibros.com/libro-cronicas-marcianas/298202) / P | Pendiente; predeterminada local |
| El hombre ilustrado | `9788445006801` | `sin-portada.svg` | [Ficha](https://www.planetadelibros.com/libro-el-hombre-ilustrado/298208) / P | Pendiente; predeterminada local |
| Soy leyenda | `9788445006764` | `sin-portada.svg` | [Ficha](https://www.planetadelibros.com/libro-soy-leyenda/298204) / P | Pendiente; predeterminada local |
| El hobbit | `9788445012802` | `sin-portada.svg` | [Ficha](https://www.planetadelibros.com/libro-el-hobbit/356019?soporte=347061) / P | Pendiente; predeterminada local |
| La Comunidad del Anillo | `9788445013557` | `sin-portada.svg` | [Ficha](https://www.planetadelibros.com/libro-el-senor-de-los-anillos-1-la-comunidad-del-anillo/356020?soporte=353810) / P | Pendiente; predeterminada local |
| Las Dos Torres | `9788445013564` | `sin-portada.svg` | [Ficha](https://www.planetadelibros.com/libro-el-senor-de-los-anillos-n-0203-las-dos-torres-ne/324395?soporte=353811) / P | Pendiente; predeterminada local |
| El Silmarillion | `9788445014493` | `sin-portada.svg` | [Ficha](https://www.planetadelibros.com/libro-el-silmarillion-edicion-revisada/349855?soporte=362733) / P | Pendiente; predeterminada local |
| El fin de la eternidad | `9788497933537` | `sin-portada.svg` | [Ficha](https://www.penguinlibros.com/es/libros-de-ciencia-ficcion/35161-libro-el-fin-de-la-eternidad-9788497933537) / R | Pendiente; predeterminada local |
| La metamorfosis | `9788420651361` | `sin-portada.svg` | [Ficha](https://www.alianzaeditorial.es/libro/bibliotecas-de-autor/la-metamorfosis-franz-kafka-9788420651361/) / A | Pendiente; predeterminada local |
| El retrato de Dorian Gray | `9788420654935` | `sin-portada.svg` | [Ficha](https://www.alianzaeditorial.es/libro/bibliotecas-de-autor/el-retrato-de-dorian-gray-oscar-wilde-9788420654935/) / A | Pendiente; predeterminada local |
| Drácula | `9788491819103` | `sin-portada.svg` | [Ficha](https://www.alianzaeditorial.es/libro/literatura/dracula-bram-stoker-9788491819103/) / A | Pendiente; predeterminada local |
| La Isla del Tesoro | `9788420651385` | `sin-portada.svg` | [Ficha](https://www.alianzaeditorial.es/libro/bibliotecas-de-autor/la-isla-del-tesoro-robert-louis-stevenson-9788420651385/) / A | Pendiente; predeterminada local |
| Frankenstein | `9788413623689` | `sin-portada.svg` | [Ficha](https://www.alianzaeditorial.es/libro/13-20/frankenstein-mary-w-shelley-9788413623689/) / A | Pendiente; predeterminada local |
| Orgullo y prejuicio | `9791370093068` | `sin-portada.svg` | [Ficha](https://alianzaeditorial.es/libro/alianza-clasicos/orgullo-y-prejuicio-jane-austen-9791370093068/) / A | Pendiente; predeterminada local |
| Alicia en el País de las Maravillas | `9788420664286` | `sin-portada.svg` | [Ficha](https://alianzaeditorial.es/libro/literatura/alicia-en-el-pais-de-las-maravillas-lewis-carroll-9788420664286/) / A | Pendiente; predeterminada local |
| Mujercitas | `9788413628448` | `sin-portada.svg` | [Ficha](https://www.alianzaeditorial.es/libro/13-20/mujercitas-louisa-may-alcott-9788413628448/) / A | Pendiente; predeterminada local |
| Odisea | `9788420674629` | `sin-portada.svg` | [Ficha](https://www.alianzaeditorial.es/libro/clasicos-de-grecia-y-roma/odisea-homero-9788420674629/) / A | Pendiente; predeterminada local |
| Ilíada | `9788420678344` | `sin-portada.svg` | [Ficha](https://www.alianzaeditorial.es/libro/clasicos-de-grecia-y-roma/iliada-homero-9788420678344/) / A | Pendiente; predeterminada local |

## Reproducir las asociaciones en SQL Server

La instalación nueva ya asigna la imagen local predeterminada mediante `scripts/libros-reales.sql`. Ahora el script también lista las 20 asociaciones al finalizar. No requiere Internet para mostrar esas imágenes.

Para revisar una base existente y completar exclusivamente rutas vacías de estos 20 libros:

```powershell
sqlcmd -S '.\SQLEXPRESS' -d EditorialDB -E -C -b -f 65001 -i scripts/asociar-portadas-locales.sql
```

El script empareja por ISBN y título, valida que estén los 20 registros y utiliza una transacción. No cambia una ruta no vacía, ni precios, inventario, ISBN, usuarios u otras tablas. Una ruta personalizada se informa como conservada; no se certifica automáticamente que sea correcta. Puede repetirse. En la base revisada se esperan **0 actualizaciones**, porque las 20 rutas ya apuntan al marcador local.

Cuando se consiga una portada autorizada: conservar la evidencia del permiso (incluida su redistribución), comprobar la edición exacta y guardar `{ISBN}.jpg` o `{ISBN}.webp` en `wwwroot/images/portadas/`. Usar aproximadamente 400 × 600 píxeles, conservar la proporción y los avisos de derechos, sin recortar ni rediseñar. Optimizar solo dentro de las condiciones del permiso. Registrar fuente, permiso, dimensiones y hash en el inventario y asociar `/images/portadas/{ISBN}.jpg` mediante **Libros → Editar**, usando el mecanismo existente. Actualizar también el script inicial con esa ruta concreta y comprobar que el archivo viaje con el proyecto antes de ejecutar SQL. No asociar nombres de archivos que todavía no existen.

## Comprobación

- `dotnet build PlataformaEditorial.slnx --no-restore`: correcto, 0 errores y 0 advertencias.
- `dotnet test PlataformaEditorial.slnx --no-build --no-restore`: 39 correctas, 0 fallidas y 0 omitidas. La suite existente usa transacciones revertidas para sus escenarios SQL; no se añadieron pruebas que dupliquen la implementación.
- Comprobación HTTP de 43 páginas con respuesta 200: inicio, dos páginas del catálogo, 20 búsquedas por ISBN y 20 detalles. Las 74 referencias a imágenes de esas páginas son locales y sus archivos existen. Se comprobó título/ISBN/ruta por cada libro contra SQL, el CSV y el archivo físico. La respuesta HTTP de la imagen coincide byte a byte con el archivo del proyecto.
- Revisión visual en navegador: inicio, catálogo, resultado por ISBN y detalles muestran el marcador local. La comprobación automatizada cubrió los 20 libros; la inspección visual utilizó páginas representativas.
- `scripts/asociar-portadas-locales.sql` ejecutado dos veces: 0 rutas modificadas en ambas; las 20 asociaciones ya eran válidas para el marcador. No se ejecutaron migraciones ni cambios de esquema.
- `scripts/libros-reales.sql` comprobado sobre los datos existentes dentro de una transacción exterior revertida: listó las 20 rutas. Se ejecutó desde archivo UTF-8 con `sqlcmd -f 65001 -i`, como indica README. Un intento previo enviando el texto por una tubería produjo un conflicto por codificación y se revirtió; no usar esa tubería para cargar textos acentuados. No se comprobó una carga desde cero en una base nueva.
- `git check-ignore` no excluye el marcador, `.gitkeep`, ni nombres JPG/WebP basados en ISBN en la carpeta reservada. El marcador ya estaba versionado; no existen 20 imágenes JPG/WebP autorizadas para incorporar.
- `git diff --check`: correcto.

SHA-256 del recurso local verificado: `5475b3536be7ca7b2c17dd95cd5ddfb14de19131e25690031a8432936f3ed118`.

No se desactivó la conexión de Windows: se comprobó que todas las referencias de portada apuntan al servidor local y que sus bytes están incluidos en el proyecto. Esto acredita la independencia de Internet de estas imágenes, no una prueba de instalación completa sin conexión.

Archivos creados: `docs/PORTADAS-LIBROS.csv`, este informe, `scripts/asociar-portadas-locales.sql` y `PlataformaEditorial.Web/wwwroot/images/portadas/.gitkeep`. Archivos modificados: `README.md`, `docs/PORTADAS-Y-FUENTES.md` y `scripts/libros-reales.sql` (comentarios y listado final de asociaciones). Se conserva el mecanismo MVC existente: SQL guarda la ruta en `tblLibro.portadaURL`, el servicio la entrega y las vistas la muestran; no hubo cambios en controladores, servicios ni vistas.

Para repetir la revisión manual: ejecutar la aplicación con `dotnet run --project PlataformaEditorial.Web --launch-profile http`, visitar `http://localhost:5172/`, `/Catalogo` y la segunda página, buscar cada ISBN del inventario y abrir su detalle. Usar el enlace del resultado para obtener el ID real de cada instalación; no asumir que los IDs coinciden entre computadoras. Debe verse la imagen predeterminada sin iconos de imagen rota. Ejecutar los dos comandos de compilación/pruebas anteriores y el script de asociaciones para comprobar la base.

Para la entrega en ZIP deben incluirse `wwwroot`, los scripts y la documentación. La base SQL no viaja automáticamente con el código: configurar SQL Server y cargar los scripts según README. Restaurar NuGet puede necesitar conexión la primera vez; las imágenes predeterminadas no la necesitan. No se ha verificado una instalación completa en una segunda computadora.
