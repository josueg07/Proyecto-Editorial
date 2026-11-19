# Fuentes y portadas

Revisión actualizada del 19/11/2026: [20 portadas locales proporcionadas por el usuario, asociadas y verificadas](REVISION-PORTADAS.md), con [CSV por ISBN y hash](PORTADAS-LIBROS.csv). Esta actualización reemplaza el estado anterior de imágenes pendientes. No se buscaron ni descargaron imágenes.

`FUENTES-LIBROS.csv` registra 20 títulos, 20 ISBN impresos y tres ISBN digitales. Las fuentes enlazadas son fichas de las editoriales, consultadas el 13/11/2026. Se comprobó además el dígito de control de cada ISBN: esto complementa, pero no sustituye, la correspondencia editorial de la ficha.

La fecha de la edición de *El fin de la eternidad* se corroboró también en [Casa del Libro](https://www.casadellibro.com/libro-el-fin-de-la-eternidad/9788497933537/970833). El año mostrado por el modelo corresponde a la edición impresa; no hay una columna de año por presentación para las ediciones digitales. Las categorías son clasificaciones del proyecto y las sinopsis se redactaron para este catálogo, sin copiar las descripciones editoriales.

Los precios e inventarios son exclusivamente académicos y no representan precios ni existencias de las editoriales. Se incorporan 120 ejemplares por edición impresa nueva. Repetir el script no repone inventario ni sobrescribe precios. Los formatos digitales no incluyen archivos de las obras ni una licencia de distribución; este proyecto registra su pedido para coordinación manual, sin simular descargas.

Las 20 imágenes aportadas están en `PlataformaEditorial.Web/wwwroot/images/portadas/{ISBN}.jpg`. Ambos scripts SQL registran las rutas `/images/portadas/{ISBN}.jpg` mediante `tblLibro.portadaURL`. Debe incluirse la carpeta completa en el ZIP. Para reemplazos futuros, comprobar el ISBN y conservar la extensión real, actualizando los scripts y el inventario. Se conserva `/images/sin-portada.svg` para los libros sin imagen propia.

La fuente de Orgullo y prejuicio corresponde a la edición de septiembre de 2026 con ilustraciones de C. E. Brock; la autoría literaria sigue siendo Jane Austen. Se registran autores literarios y se conservan los traductores e ilustradores en las fichas enlazadas, porque el modelo actual no distingue esas funciones.
